using EDAN.Tensor.HostServices.Services;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;

namespace EDAN.Tensor.WorkerHost.BackgroundServices;

/// <summary>5분 간격으로 Tenant별 스냅샷 작업을 멱등하게 WorkerTask 큐에 등록합니다.</summary>
public sealed class DataSetStorageUsageSnapshotWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<DataSetStorageUsageSnapshotWorker> logger,
    TimeProvider timeProvider) : BackgroundService
{
    public const string TaskType = "dataset.storage-usage.snapshot";
    public const string PayloadReference = "dataset-storage-usage-snapshot:v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5), timeProvider);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EnqueueDueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Tenant 저장량 스냅샷 작업 등록에 실패했습니다."); }

            try { if (!await timer.WaitForNextTickAsync(stoppingToken)) break; }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task EnqueueDueAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var snapshotService = scope.ServiceProvider.GetRequiredService<DataSetStorageUsageSnapshotService>();
        var enqueueService = scope.ServiceProvider.GetRequiredService<WorkerTaskEnqueueService>();
        var now = timeProvider.GetUtcNow();
        var bucket = now.ToUnixTimeSeconds() / 300;
        var tenantIds = await snapshotService.GetTenantIdsAsync(cancellationToken);
        var created = 0;
        foreach (var tenantId in tenantIds)
        {
            var result = await enqueueService.EnqueueAsync(
                tenantId,
                $"dataset-storage-snapshot:{tenantId:N}:{bucket}",
                TaskType,
                PayloadReference,
                maxAttempts: 5,
                cancellationToken);
            if (result.Conflict)
                logger.LogWarning("스냅샷 작업 Idempotency 충돌: TenantId={TenantId}, TaskId={TaskId}", tenantId, result.Task.Id);
            if (result.Created) created++;
        }
        if (created > 0) logger.LogInformation("Tenant 저장량 스냅샷 WorkerTask {Count}건을 큐에 등록했습니다.", created);
    }
}
