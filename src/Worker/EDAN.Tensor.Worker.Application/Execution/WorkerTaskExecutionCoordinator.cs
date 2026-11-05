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
    public static WorkerTaskExecutionResult Success() => new(true);
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
    HandedOff,
    LeaseLost
}

/// <summary>Lease, 실행 핸들러, 종료 인계를 연결하는 단일 작업 실행 조정기입니다.</summary>
public sealed class WorkerTaskExecutionCoordinator(
    IWorkerTaskRepository repository,
    IWorkerTaskHandlerResolver handlers,
    WorkerExecutionRegistry executions,
    TimeProvider timeProvider)
{
    public async Task<WorkerTaskExecutionOutcome> ExecuteAsync(
        Guid tenantId, Guid taskId, string workerId, TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));

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
        if (!await repository.TryStartAsync(tenantId, taskId, leaseId, fencingToken, now, cancellationToken))
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
                result = await execution.Completion.WaitAsync(cancellationToken);
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
            return await repository.TryFailAsync(
                tenantId, taskId, leaseId, fencingToken, errorCode, now, retryAt, cancellationToken)
                ? WorkerTaskExecutionOutcome.RetryScheduled
                : WorkerTaskExecutionOutcome.LeaseLost;
        }
    }
}
