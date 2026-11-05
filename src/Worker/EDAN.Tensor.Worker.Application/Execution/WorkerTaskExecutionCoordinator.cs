using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>작업 종류에 맞는 실행 핸들러를 찾습니다.</summary>
public interface IWorkerTaskHandlerResolver
{
    IWorkerTaskHandler? Resolve(string taskType);
}

/// <summary>개별 작업의 실제 비즈니스 실행을 시작합니다.</summary>
public interface IWorkerTaskHandler
{
    Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken);
}

/// <summary>실행 세션은 완료 결과와 안전한 종료 체크포인트를 제공합니다.</summary>
public interface IWorkerTaskExecution : IAsyncDisposable
{
    Task<WorkerTaskExecutionResult> Completion { get; }
    Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken);
}

/// <summary>작업 실행의 성공 또는 재시도 가능한 실패 결과입니다.</summary>
public sealed record WorkerTaskExecutionResult(bool Succeeded, string? ErrorCode = null,
    DateTimeOffset? RetryAtUtc = null)
{
    public string? ResultReference { get; init; }\n    public static WorkerTaskExecutionResult Success(string? resultReference = null) => new(true) { ResultReference = resultReference };
    public static WorkerTaskExecutionResult Failure(string errorCode, DateTimeOffset retryAtUtc) =>
        new(false, errorCode, retryAtUtc);
}

/// <summary>작업 실행 조정 결과입니다.</summary>
public enum WorkerTaskExecutionOutcome
{
    NotFound,
    NoHandler,
    LeaseUnavailable,
    Started,
    Succeeded,
    RetryScheduled,
    DeadLettered,
    HandedOff,
    LeaseLost
}

/// <summary>Lease 갱신, 실행 핸들러, 종료 인계를 연결하는 작업 실행 조정기입니다.</summary>
public sealed class WorkerTaskExecutionCoordinator(
    IWorkerTaskRepository repository,
    IWorkerTaskHandlerResolver handlers,
    IWorkerTaskLeaseRenewer leaseRenewer,
    WorkerExecutionRegistry executions,
    TimeProvider timeProvider)
{
    public async Task<WorkerTaskExecutionOutcome> ExecuteAsync(
        Guid tenantId, Guid taskId, string workerId, TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (leaseDuration < TimeSpan.FromSeconds(3))
            throw new ArgumentOutOfRangeException(nameof(leaseDuration), "Lease 기간은 3초 이상이어야 합니다.");

        var task = await repository.GetAsync(tenantId, taskId, cancellationToken);
        if (task is null)
            return WorkerTaskExecutionOutcome.NotFound;

        var handler = handlers.Resolve(task.TaskType);
        if (handler is null)
            return WorkerTaskExecutionOutcome.NoHandler;

        var now = timeProvider.GetUtcNow();
        var leaseId = Guid.NewGuid();
        var leased = await repository.TryAcquireLeaseAsync(
            tenantId, taskId, workerId, leaseId, now, now.Add(leaseDuration), cancellationToken);
        if (leased is null)
            return WorkerTaskExecutionOutcome.LeaseUnavailable;

        var fencingToken = leased.FencingToken;
        now = timeProvider.GetUtcNow();
        if (!await repository.TryStartAsync(tenantId, taskId, leaseId, fencingToken, now, result.ResultReference, cancellationToken))
            return WorkerTaskExecutionOutcome.LeaseLost;

        await using var execution = await handler.StartAsync(leased, cancellationToken);
        var handedOff = 0;

        async Task<bool> HandoffAsync(CancellationToken token)
        {
            var checkpoint = await execution.RequestCheckpointAndStopAsync(token);
            if (string.IsNullOrWhiteSpace(checkpoint))
                return false;

            var saved = await repository.TryHandoffAsync(
                tenantId, taskId, leaseId, fencingToken, checkpoint, timeProvider.GetUtcNow(), token);
            if (saved)
                Interlocked.Exchange(ref handedOff, 1);
            return saved;
        }

        if (!executions.TryEnter(HandoffAsync, out var executionLease))
        {
            return await HandoffAsync(cancellationToken)
                ? WorkerTaskExecutionOutcome.HandedOff
                : WorkerTaskExecutionOutcome.LeaseLost;
        }

        using (executionLease)
        {
            WorkerTaskExecutionResult result;
            try
            {
                var completion = execution.Completion;
                var heartbeatInterval = TimeSpan.FromTicks(Math.Clamp(
                    leaseDuration.Ticks / 3,
                    TimeSpan.FromSeconds(1).Ticks,
                    TimeSpan.FromMinutes(1).Ticks));

                while (!completion.IsCompleted)
                {
                    var delay = Task.Delay(heartbeatInterval, timeProvider, cancellationToken);
                    if (await Task.WhenAny(completion, delay) == completion)
                        break;

                    await delay;
                    now = timeProvider.GetUtcNow();
                    var renewed = await leaseRenewer.RenewAsync(
                        tenantId, taskId, leaseId, fencingToken, now, now.Add(leaseDuration), cancellationToken);
                    if (!renewed)
                    {
                        await execution.RequestCheckpointAndStopAsync(CancellationToken.None);
                        return WorkerTaskExecutionOutcome.LeaseLost;
                    }
                }

                result = await completion.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                result = WorkerTaskExecutionResult.Failure(
                    "worker.execution.failed", timeProvider.GetUtcNow().AddMinutes(1));
            }

            if (Volatile.Read(ref handedOff) == 1)
                return WorkerTaskExecutionOutcome.HandedOff;

            now = timeProvider.GetUtcNow();
            if (result.Succeeded)
            {
                return await repository.TryCompleteAsync(
                    tenantId, taskId, leaseId, fencingToken, now, cancellationToken)
                    ? WorkerTaskExecutionOutcome.Succeeded
                    : WorkerTaskExecutionOutcome.LeaseLost;
            }

            var retryAt = result.RetryAtUtc ?? now.AddMinutes(1);
            var errorCode = string.IsNullOrWhiteSpace(result.ErrorCode)
                ? "worker.execution.failed"
                : result.ErrorCode;
            var failed = await repository.TryFailAsync(
                tenantId, taskId, leaseId, fencingToken, errorCode, now, retryAt, cancellationToken);
            if (!failed)
                return WorkerTaskExecutionOutcome.LeaseLost;

            // 최대 시도 횟수에 도달한 작업은 Domain에서 DLQ로 전이되므로
            // 영속 상태를 다시 읽어 실제 결과를 Dispatcher 호출자에게 반환합니다.
            var failedTask = await repository.GetAsync(tenantId, taskId, cancellationToken);
            return failedTask?.Status == WorkerTaskStatus.DeadLettered
                ? WorkerTaskExecutionOutcome.DeadLettered
                : WorkerTaskExecutionOutcome.RetryScheduled;
        }
    }
}
