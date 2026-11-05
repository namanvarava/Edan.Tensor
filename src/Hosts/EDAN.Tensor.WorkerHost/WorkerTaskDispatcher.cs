using System.Collections.Concurrent;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

internal sealed class WorkerTaskDispatcher(
    ILogger<WorkerTaskDispatcher> logger,
    IServiceScopeFactory scopeFactory,
    WorkerExecutionRegistry executions,
    WorkerTaskHandlerRegistry handlers,
    IConfiguration configuration,
    TimeProvider timeProvider) : BackgroundService
{
    private readonly ConcurrentDictionary<string, byte> _active = new(StringComparer.Ordinal);
    private readonly object _activeSync = new();
    private TaskCompletionSource _activeDrained = CompletedSource();
    private readonly string _workerId = $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";
    private Guid? _tenantAfterId;
    private readonly ConcurrentDictionary<Guid, Guid> _tenantTaskAfterIds = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollSeconds = Math.Clamp(configuration.GetValue("Worker:PollIntervalSeconds", 5), 1, 60);
        logger.LogInformation("Worker 작업 디스패처 시작: WorkerId={WorkerId}", _workerId);
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await DispatchAvailableAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Worker 작업 검색 또는 디스패치 중 오류가 발생했습니다."); }
            try { await Task.Delay(TimeSpan.FromSeconds(pollSeconds), timeProvider, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task DispatchAvailableAsync(CancellationToken cancellationToken)
    {
        var maxConcurrent = Math.Clamp(configuration.GetValue("Worker:MaxConcurrentTasks", 4), 1, 128);
        var slots = maxConcurrent - _active.Count;
        if (slots <= 0) return;
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkerTaskRepository>();
        var now = timeProvider.GetUtcNow();
        var recoveryBatchSize = Math.Clamp(configuration.GetValue("Worker:LeaseRecoveryBatchSize", 100), 1, 500);
        var recovered = await repository.RecoverExpiredLeasesAsync(now, recoveryBatchSize, cancellationToken);
        if (recovered > 0) logger.LogWarning("만료된 Worker Lease {Count}건을 복구했습니다.", recovered);
        const int tenantPageSize = 500;
        var tenantIds = await repository.GetReadyTenantIdsAsync(now, tenantPageSize, cancellationToken, _tenantAfterId);
        if (tenantIds.Count == 0 && _tenantAfterId.HasValue)
        {
            _tenantAfterId = null;
            tenantIds = await repository.GetReadyTenantIdsAsync(now, tenantPageSize, cancellationToken);
        }
        if (tenantIds.Count == 0) return;
        var candidateBatchSize = Math.Clamp(configuration.GetValue("Worker:TenantCandidateBatchSize", 16), 1, 100);
        var maxCandidateScan = Math.Clamp(configuration.GetValue("Worker:TenantCandidateScanLimit", 500), candidateBatchSize, 5000);
        foreach (var tenantId in tenantIds)
        {
            if (slots <= 0) break;
            _tenantAfterId = tenantId;
            WorkerTask? task = null;
            Guid? taskAfterId = _tenantTaskAfterIds.TryGetValue(tenantId, out var cursor) ? cursor : null;
            var scanned = 0;
            var reachedEnd = false;
            while (scanned < maxCandidateScan)
            {
                var take = Math.Min(candidateBatchSize, maxCandidateScan - scanned);
                var tasks = await repository.GetReadyAsync(tenantId, now, take, cancellationToken, taskAfterId);
                if (tasks.Count == 0)
                {
                    if (taskAfterId.HasValue)
                    {
                        taskAfterId = null;
                        continue;
                    }
                    reachedEnd = true;
                    break;
                }

                foreach (var candidate in tasks)
                {
                    scanned++;
                    taskAfterId = candidate.Id;
                    if (handlers.Resolve(candidate.TaskType) is null) continue;
                    task = candidate;
                    break;
                }
                if (task is not null) break;
                if (tasks.Count < take)
                {
                    reachedEnd = true;
                    taskAfterId = null;
                    break;
                }
            }

            if (reachedEnd || !taskAfterId.HasValue)
                _tenantTaskAfterIds.TryRemove(tenantId, out _);
            else
                _tenantTaskAfterIds[tenantId] = taskAfterId.Value;

            if (task is null) continue;
            var key = $"{tenantId:N}:{task.Id:N}";
            if (!TryMarkActive(key)) continue;
            slots--;
            _ = RunTaskAsync(key, tenantId, task.Id);
        }
    }

    private bool TryMarkActive(string key)
    {
        lock (_activeSync)
        {
            if (!_active.TryAdd(key, 0)) return false;
            if (_active.Count == 1) _activeDrained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return true;
        }
    }
    private void RemoveActive(string key)
    {
        lock (_activeSync)
        {
            _active.TryRemove(key, out _);
            if (_active.IsEmpty) _activeDrained.TrySetResult();
        }
    }
    private async Task RunTaskAsync(string key, Guid tenantId, Guid taskId)
    {
        await Task.Yield();
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var coordinator = scope.ServiceProvider.GetRequiredService<WorkerTaskExecutionCoordinator>();
            var leaseSeconds = Math.Clamp(configuration.GetValue("Worker:LeaseSeconds", 300), 3, 3600);
            var outcome = await coordinator.ExecuteAsync(tenantId, taskId, _workerId, TimeSpan.FromSeconds(leaseSeconds), CancellationToken.None);
            logger.LogInformation("Worker 작업 실행 종료: TenantId={TenantId}, TaskId={TaskId}, Outcome={Outcome}", tenantId, taskId, outcome);
        }
        catch (Exception exception) { logger.LogError(exception, "Worker 작업 실행 중 예외가 발생했습니다. TenantId={TenantId}, TaskId={TaskId}", tenantId, taskId); }
        finally { RemoveActive(key); }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        var graceSeconds = Math.Clamp(configuration.GetValue("Worker:ShutdownGraceSeconds", 30), 1, 300);
        var handoffSeconds = Math.Clamp(configuration.GetValue("Worker:ShutdownHandoffSeconds", 15), 1, 120);
        var result = await executions.StopAsync(TimeSpan.FromSeconds(graceSeconds), TimeSpan.FromSeconds(handoffSeconds), cancellationToken);
        Task activeDrained;
        lock (_activeSync) activeDrained = _activeDrained.Task;
        try { await activeDrained.WaitAsync(TimeSpan.FromSeconds(handoffSeconds), cancellationToken); }
        catch (TimeoutException) { logger.LogWarning("종료 제한 시간 내 완료되지 않은 디스패치가 있습니다. Active={Active}", _active.Count); }
        logger.LogInformation("Worker 종료 조정 완료: 유예 시간 내 정리={Drained}, 인계 요청={Requested}, 인계 성공={Succeeded}, 실행 잔여={Remaining}",
            result.DrainedWithinGracePeriod, result.HandoffRequested, result.HandoffSucceeded, result.StillRunning);
    }
    private static TaskCompletionSource CompletedSource()
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        source.SetResult();
        return source;
    }
}
