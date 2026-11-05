using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>테넌트별 감사 아카이브 예약 정책과 실행 상태를 관리합니다.</summary>
public sealed class AuthorizationAuditArchiveScheduleService(
    IAuthorizationAuditArchiveScheduleRepository schedules,
    AuthorizationAuditArchiveJobService jobs,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock)
{
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

    /// <summary>기한이 도래한 예약을 임대하고 아카이브 작업을 실행합니다.</summary>
    public async Task<int> RunDueAsync(CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var due = await schedules.GetDueTenantsAsync(now, 20, cancellationToken);
        var executed = 0;
        foreach (var tenantId in due)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var leaseId = Guid.NewGuid();
            if (!await schedules.TryAcquireAsync(tenantId, leaseId, now, now.AddMinutes(30), cancellationToken))
                continue;

            var schedule = await schedules.GetAsync(tenantId, cancellationToken);
            var fromUtc = schedule?.LastRunAtUtc ?? now.AddHours(-(schedule?.IntervalHours ?? 24));
            var toUtc = now;
            var succeeded = false;
            try
            {
                await jobs.CreateAsync(tenantId, fromUtc, toUtc, "system:archive-scheduler", cancellationToken);
                succeeded = true;
                executed++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // 실패한 실행은 다음 주기를 5분 뒤로 예약하고 원본 감사 데이터는 유지합니다.
            }
            finally
            {
                await schedules.CompleteAsync(tenantId, leaseId, clock.UtcNow, succeeded, cancellationToken);
            }
        }
        return executed;
    }

    private static AuthorizationAuditArchiveScheduleView ToView(AuthorizationAuditArchiveSchedule x) =>
        new(x.TenantId.Value, x.IsEnabled, x.IntervalHours, x.NextRunAtUtc, x.LastRunAtUtc, x.Version);
}

/// <summary>테넌트 감사 아카이브 예약 정책을 제공합니다.</summary>
public sealed record AuthorizationAuditArchiveScheduleView(Guid TenantId, bool IsEnabled,
    int IntervalHours, DateTimeOffset? NextRunAtUtc, DateTimeOffset? LastRunAtUtc, long Version);
