using EDAN.Tensor.ApiHost.Services;

namespace EDAN.Tensor.ApiHost.BackgroundServices;

/// <summary>5분 간격으로 Tenant 저장량을 확인하되 시간 구간당 스냅샷은 하나만 유지합니다.</summary>
public sealed class DataSetStorageUsageSnapshotWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<DataSetStorageUsageSnapshotWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<DataSetStorageUsageSnapshotService>();
                var count = await service.CaptureDueAsync(stoppingToken);
                if (count > 0) logger.LogInformation("Tenant 저장량 스냅샷 {Count}건을 기록했습니다.", count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Tenant 저장량 스냅샷 기록에 실패했습니다."); }

            try { if (!await timer.WaitForNextTickAsync(stoppingToken)) break; }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
