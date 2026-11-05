using EDAN.Tensor.Platform.Application.UseCases;

namespace EDAN.Tensor.ApiHost.BackgroundServices;

/// <summary>예약된 감사 아카이브를 주기적으로 실행하는 호스트 작업자입니다.</summary>
public sealed class AuthorizationAuditArchiveSchedulerWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AuthorizationAuditArchiveSchedulerWorker> logger) : BackgroundService
{
    /// <summary>1분 간격으로 기한이 도래한 Tenant 예약을 확인하고 실행합니다.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<AuthorizationAuditArchiveScheduleService>();
                var executed = await scheduler.RunDueAsync(stoppingToken);
                if (executed > 0)
                    logger.LogInformation("예약 감사 아카이브 {Count}건을 실행했습니다.", executed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "예약 감사 아카이브 실행 중 오류가 발생했습니다.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
