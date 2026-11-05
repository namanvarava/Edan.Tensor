using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>실제 콘텐츠 DB 사용량 스냅샷과 Quota 임계치 상태 전이를 멱등 기록합니다.</summary>
public sealed class DataSetStorageUsageSnapshotService(
    DataSetDbContext dataSetDb,
    PlatformDbContext platformDb,
    IDataSetTenantStorageUsageReader usageReader,
    TimeProvider timeProvider)
{
    public async Task<int> CaptureDueAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var period = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero);
        var tenantIds = await dataSetDb.DataSets.AsNoTracking()
            .Select(x => x.TenantId).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);

        foreach (var tenantId in tenantIds)
        {
            var usage = await usageReader.GetTenantUsageAsync(tenantId, cancellationToken);
            var snapshot = await dataSetDb.StorageUsageSnapshots.SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.PeriodStartUtc == period, cancellationToken);
            if (snapshot is null)
            {
                dataSetDb.StorageUsageSnapshots.Add(new DataSetStorageUsageSnapshot
                {
                    TenantId = tenantId, PeriodStartUtc = period, CapturedAtUtc = now,
                    UsedBytes = usage.TenantBytes, DataSetCount = usage.TenantDataSetCount
                });
            }
            else
            {
                snapshot.CapturedAtUtc = now;
                snapshot.UsedBytes = usage.TenantBytes;
                snapshot.DataSetCount = usage.TenantDataSetCount;
            }
            await EvaluateTenantPolicyAsync(tenantId, usage.TenantBytes, now, cancellationToken);
        }
        await dataSetDb.SaveChangesAsync(cancellationToken);
        return tenantIds.Count;
    }

    /// <summary>현재 상태와 다른 상태로 바뀔 때만 이벤트를 추가해 주기 실행의 중복 기록을 막습니다.</summary>
    private async Task EvaluateTenantPolicyAsync(Guid tenantId, long usedBytes, DateTimeOffset now, CancellationToken token)
    {
        var policy = await platformDb.StorageQuotaPolicies.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TenantId == new TenantId(tenantId)
                && x.Scope == StorageQuotaPolicyScope.Tenant && x.TargetId == tenantId && x.IsEnabled, token);
        if (policy?.TenantLimitBytes is not long limitBytes)
            return;

        var utilization = limitBytes == 0
            ? (usedBytes == 0 ? 0d : 100d)
            : Math.Round(usedBytes * 100d / limitBytes, 2);
        var currentState = usedBytes >= limitBytes
            ? "Exceeded"
            : utilization >= policy.WarningThresholdPercent ? "Warning" : "Normal";
        var latest = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PolicyId == policy.Id)
            .OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id)
            .FirstOrDefaultAsync(token);
        var previousState = latest?.PolicyVersion == policy.Version ? latest.EventType : "Normal";
        if (currentState == previousState)
            return;
        if (currentState == "Normal" && latest is null)
            return;

        dataSetDb.QuotaThresholdEvents.Add(new DataSetQuotaThresholdEvent
        {
            TenantId = tenantId, PolicyId = policy.Id, Scope = policy.Scope.ToString(),
            TargetId = policy.TargetId, EventType = currentState == "Normal" ? "Resolved" : currentState,
            UsedBytes = usedBytes, LimitBytes = limitBytes, UtilizationPercent = utilization,
            PolicyVersion = policy.Version, OccurredAtUtc = now
        });
    }

    public Task<List<DataSetStorageUsageSnapshot>> GetHistoryAsync(
        Guid tenantId, int take = 168, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        return dataSetDb.StorageUsageSnapshots.AsNoTracking().Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.PeriodStartUtc).Take(Math.Clamp(take, 1, 744)).ToListAsync(cancellationToken);
    }

    public Task<List<DataSetQuotaThresholdEvent>> GetQuotaEventHistoryAsync(
        Guid tenantId, int take = 100, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        return dataSetDb.QuotaThresholdEvents.AsNoTracking().Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id)
            .Take(Math.Clamp(take, 1, 500)).ToListAsync(cancellationToken);
    }
}
