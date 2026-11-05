using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>테넌트별 감사 아카이브 예약 정책과 실행 상태를 관리합니다.</summary>
public sealed class AuthorizationAuditArchiveScheduleService(
    IAuthorizationAuditArchiveScheduleRepository schedules,
    AuthorizationAuditArchiveJobService jobs,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock,
    IServiceScopeFactory scopeFactory)
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan LeaseRenewalInterval = TimeSpan.FromMinutes(5);
    /// <summary>예약 정책을 조회하고 미설정 상태를 기본값으로 반환합니다.</summary>
    public async Task<AuthorizationAuditArchiveScheduleView> GetAsync(TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await schedules.GetAsync(tenantId, cancellationToken);
        return schedule is null
            ? new AuthorizationAuditArchiveScheduleView(tenantId.Value, false, 24, null, null, 0)
            : ToView(schedule);
    }

    /// <summary>예약 주기와 활성 상태를 저장합니다.</summary>
    public async Task<AuthorizationAuditArchiveScheduleView> UpdateAsync(TenantId tenantId,
        int intervalHours, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var schedule = await schedules.GetAsync(tenantId, cancellationToken);
        if (schedule is null)
        {
            schedule = AuthorizationAuditArchiveSchedule.Create(tenantId, intervalHours, clock.UtcNow, isEnabled);
            schedules.Add(schedule);
        }
        else
        {
            schedule.Update(intervalHours, isEnabled, clock.UtcNow);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToView(schedule);
    }

    /// <summary>기한이 도래한 예약을 임대하고 Heartbeat로 임대를 유지하며 실행합니다.</summary>
    public async Task<int> RunDueAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var due = await schedules.GetDueTenantsAsync(now, 20, cancellationToken);
        var executed = 0;
        foreach (var tenantId in due)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var leaseId = Guid.NewGuid();
            if (!await schedules.TryAcquireAsync(tenantId, leaseId, now,
                    now.Add(LeaseDuration), cancellationToken))
                continue;

            var schedule = await schedules.GetAsync(tenantId, cancellationToken);
            var fromUtc = schedule?.LastRunAtUtc?.AddTicks(1)
                ?? now.AddHours(-(schedule?.IntervalHours ?? 24));
            var toUtc = fromUtc.AddDays(31) < now ? fromUtc.AddDays(31) : now;
            var succeeded = false;
            var leaseLost = false;
            using var executionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var heartbeatTask = MaintainLeaseAsync(tenantId, leaseId, executionCts,
                () => leaseLost = true);

            try
            {
                await jobs.CreateAsync(tenantId, fromUtc, toUtc,
                    "system:archive-scheduler", executionCts.Token);
                if (!leaseLost)
                {
                    succeeded = true;
                    executed++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // 실패한 실행은 다음 주기를 5분 뒤로 예약하고 원본 감사 데이터는 유지합니다.
            }
            finally
            {
                executionCts.Cancel();
                await heartbeatTask;
                if (!leaseLost)
                {
                    await schedules.CompleteAsync(tenantId, leaseId, clock.UtcNow,
                        succeeded ? toUtc : null, succeeded, CancellationToken.None);
                }
            }
        }
        return executed;
    }

    /// <summary>별도 DI 범위와 DbContext로 실행 중인 임대를 주기적으로 연장합니다.</summary>
    private async Task MaintainLeaseAsync(TenantId tenantId, Guid leaseId,
        CancellationTokenSource executionCts, Action onLeaseLost)
    {
        using var timer = new PeriodicTimer(LeaseRenewalInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(executionCts.Token))
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider
                    .GetRequiredService<IAuthorizationAuditArchiveScheduleRepository>();
                var now = clock.UtcNow;
                var renewed = await repository.RenewLeaseAsync(tenantId, leaseId,
                    now, now.Add(LeaseDuration), executionCts.Token);
                if (renewed)
                    continue;

                onLeaseLost();
                executionCts.Cancel();
                return;
            }
        }
        catch (OperationCanceledException) when (executionCts.IsCancellationRequested)
        {
            // 작업 완료 또는 호스트 종료에 따른 Heartbeat 중단입니다.
        }
        catch
        {
            // 갱신 결과를 확인할 수 없으면 임대 소유권을 보장할 수 없으므로 작업을 중단합니다.
            onLeaseLost();
            executionCts.Cancel();
        }
    }

    private static AuthorizationAuditArchiveScheduleView ToView(AuthorizationAuditArchiveSchedule x) =>
        new(x.TenantId.Value, x.IsEnabled, x.IntervalHours, x.NextRunAtUtc, x.LastRunAtUtc, x.Version);
}

/// <summary>테넌트 감사 아카이브 예약 정책을 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveScheduleView(Guid TenantId, bool IsEnabled,
    int IntervalHours, DateTimeOffset? NextRunAtUtc, DateTimeOffset? LastRunAtUtc, long Version);
