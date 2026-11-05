using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.HostServices.Services;

/// <summary>실제 콘텐츠 DB 사용량 스냅샷과 Quota 임계치 상태 전이를 멱등 기록합니다.</summary>
public sealed class DataSetStorageUsageSnapshotService(
    DataSetDbContext dataSetDb,
    PlatformDbContext platformDb,
    IDataSetTenantStorageUsageReader tenantUsageReader,
    IDataSetStorageUsageReader storageUsageReader,
    TimeProvider timeProvider)
{
    public Task<int> CaptureDueAsync(CancellationToken cancellationToken = default)
        => CaptureAsync(null, cancellationToken);

    /// <summary>단일 Tenant의 사용량과 Quota 상태만 수집합니다.</summary>
    public Task<int> CaptureTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        return CaptureAsync(tenantId, cancellationToken);
    }

    /// <summary>수집 대상 Tenant 식별자를 반환합니다.</summary>
    public async Task<IReadOnlyList<Guid>> GetTenantIdsAsync(CancellationToken cancellationToken = default)
    {
        var dataSetTenantIds = await dataSetDb.DataSets.AsNoTracking()
            .Select(x => x.TenantId).Distinct().ToListAsync(cancellationToken);
        var policyTenantIds = await platformDb.StorageQuotaPolicies.AsNoTracking()
            .Select(x => x.TenantId.Value).Distinct().ToListAsync(cancellationToken);
        var eventTenantIds = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.EventType == "Warning" || x.EventType == "Exceeded")
            .Select(x => x.TenantId).Distinct().ToListAsync(cancellationToken);
        return dataSetTenantIds.Concat(policyTenantIds).Concat(eventTenantIds)
            .Distinct().OrderBy(x => x).ToArray();
    }

    private async Task<int> CaptureAsync(Guid? onlyTenantId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var period = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero);
        var dataSets = await dataSetDb.DataSets.AsNoTracking()
            .OrderBy(x => x.TenantId).ThenBy(x => x.OrganizationId).ThenBy(x => x.ProjectId)
            .ToListAsync(cancellationToken);
        var policies = await platformDb.StorageQuotaPolicies.AsNoTracking()
            .ToListAsync(cancellationToken);

        // 데이터가 아직 없는 Tenant도 활성 정책이 있으면 평가 대상에 포함합니다.
        // 삭제된 정책도 마지막 활성 이벤트가 남아 있으면 해제 전이를 기록할 수 있도록 이력을 평가 대상에 포함합니다.
        var eventTenantIds = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.EventType == "Warning" || x.EventType == "Exceeded")
            .Select(x => x.TenantId).Distinct().ToListAsync(cancellationToken);
        var activePolicyIds = policies.Select(x => x.Id).ToHashSet();
        var tenantIds = dataSets.Select(x => x.TenantId)
            .Concat(policies.Select(x => x.TenantId.Value))
            .Concat(eventTenantIds)
            .Where(x => !onlyTenantId.HasValue || x == onlyTenantId.Value)
            .Distinct().OrderBy(x => x).ToArray();

        foreach (var tenantId in tenantIds)
        {
            var usage = await tenantUsageReader.GetTenantUsageAsync(tenantId, cancellationToken);
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

            var tenantPolicies = policies.Where(x => x.TenantId.Value == tenantId)
                .OrderBy(x => x.Scope).ThenBy(x => x.TargetId);
            foreach (var policy in tenantPolicies)
            {
                var targetDataSet = policy.Scope switch
                {
                    StorageQuotaPolicyScope.Organization => dataSets.FirstOrDefault(x =>
                        x.TenantId.Value == tenantId && x.OrganizationId == policy.TargetId),
                    StorageQuotaPolicyScope.Project => dataSets.FirstOrDefault(x =>
                        x.TenantId.Value == tenantId && x.ProjectId == policy.TargetId),
                    StorageQuotaPolicyScope.DataSet => dataSets.FirstOrDefault(x =>
                        x.TenantId.Value == tenantId && x.Id.Value == policy.TargetId),
                    _ => null
                };

                long usedBytes;
                long? limitBytes;
                if (!policy.IsEnabled)
                {
                    usedBytes = 0;
                    limitBytes = null;
                }
                else if (policy.Scope == StorageQuotaPolicyScope.Tenant)
                {
                    usedBytes = usage.TenantBytes;
                    limitBytes = policy.TenantLimitBytes;
                }
                else if (targetDataSet is null)
                {
                    // 해당 범위에 콘텐츠가 없거나 대상 DataSet이 제거된 경우 사용량은 0으로 간주합니다.
                    usedBytes = 0;
                    limitBytes = policy.Scope switch
                    {
                        StorageQuotaPolicyScope.Organization => policy.OrganizationLimitBytes,
                        StorageQuotaPolicyScope.Project => policy.ProjectLimitBytes,
                        StorageQuotaPolicyScope.DataSet => policy.DataSetLimitBytes,
                        _ => null
                    };
                }
                else
                {
                    var allocation = new DataSetStorageAllocation(targetDataSet.Id, targetDataSet.Scope);
                    var scopedUsage = await storageUsageReader.GetUsageAsync(allocation, cancellationToken);
                    usedBytes = policy.Scope switch
                    {
                        StorageQuotaPolicyScope.Organization => scopedUsage.OrganizationBytes,
                        StorageQuotaPolicyScope.Project => scopedUsage.ProjectBytes,
                        StorageQuotaPolicyScope.DataSet => scopedUsage.DataSetBytes,
                        _ => 0
                    };
                    limitBytes = policy.Scope switch
                    {
                        StorageQuotaPolicyScope.Organization => policy.OrganizationLimitBytes,
                        StorageQuotaPolicyScope.Project => policy.ProjectLimitBytes,
                        StorageQuotaPolicyScope.DataSet => policy.DataSetLimitBytes,
                        _ => null
                    };
                }

                await EvaluatePolicyAsync(policy, usedBytes, limitBytes, now, cancellationToken);
            }

            await ResolveDeletedPoliciesAsync(tenantId, activePolicyIds, now, cancellationToken);
        }

        await dataSetDb.SaveChangesAsync(cancellationToken);
        return tenantIds.Length;
    }

    /// <summary>삭제된 정책의 마지막 Warning/Exceeded 상태를 한 번만 해제합니다.</summary>
    private async Task ResolveDeletedPoliciesAsync(
        Guid tenantId, IReadOnlySet<Guid> activePolicyIds, DateTimeOffset now, CancellationToken token)
    {
        var latestEvents = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.Id)
            .ToListAsync(token);

        foreach (var latest in latestEvents.GroupBy(x => x.PolicyId).Select(x => x.First()))
        {
            if (activePolicyIds.Contains(latest.PolicyId)
                || latest.EventType is not ("Warning" or "Exceeded"))
                continue;

            dataSetDb.QuotaThresholdEvents.Add(new DataSetQuotaThresholdEvent
            {
                TenantId = latest.TenantId,
                PolicyId = latest.PolicyId,
                Scope = latest.Scope,
                TargetId = latest.TargetId,
                EventType = "Resolved",
                UsedBytes = 0,
                LimitBytes = latest.LimitBytes,
                UtilizationPercent = 0,
                PolicyVersion = latest.PolicyVersion,
                OccurredAtUtc = now
            });
        }
    }

    /// <summary>Tenant·Organization·Project·DataSet 범위별 실제 사용량으로 상태 전이를 멱등 기록합니다.</summary>
    private async Task EvaluatePolicyAsync(
        StorageQuotaPolicy policy, long usedBytes, long? limitBytes, DateTimeOffset now, CancellationToken token)
    {
        var latest = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.TenantId == policy.TenantId.Value && x.PolicyId == policy.Id)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(token);

        // 정책 비활성화 또는 한도 제거는 이전 Warning/Exceeded 상태를 해제하는 전이로 기록합니다.
        if (!policy.IsEnabled || limitBytes is null)
        {
            if (latest is null || latest.EventType is "Resolved" or "Normal")
                return;
            dataSetDb.QuotaThresholdEvents.Add(new DataSetQuotaThresholdEvent
            {
                TenantId = policy.TenantId.Value, PolicyId = policy.Id, Scope = policy.Scope.ToString(),
                TargetId = policy.TargetId, EventType = "Resolved", UsedBytes = 0,
                LimitBytes = latest.LimitBytes, UtilizationPercent = 0,
                PolicyVersion = policy.Version, OccurredAtUtc = now
            });
            return;
        }

        var limit = limitBytes.Value;
        var utilization = limit == 0
            ? (usedBytes == 0 ? 0d : 100d)
            : Math.Round(usedBytes * 100d / limit, 2);
        var currentState = usedBytes >= limit
            ? "Exceeded"
            : utilization >= policy.WarningThresholdPercent ? "Warning" : "Normal";
        var previousState = latest is null || latest.EventType is "Resolved" or "Normal"
            ? "Normal" : latest.EventType;
        if (currentState == previousState || (currentState == "Normal" && latest is null))
            return;

        dataSetDb.QuotaThresholdEvents.Add(new DataSetQuotaThresholdEvent
        {
            TenantId = policy.TenantId.Value, PolicyId = policy.Id, Scope = policy.Scope.ToString(),
            TargetId = policy.TargetId, EventType = currentState == "Normal" ? "Resolved" : currentState,
            UsedBytes = usedBytes, LimitBytes = limit, UtilizationPercent = utilization,
            PolicyVersion = policy.Version, OccurredAtUtc = now
        });
    }

    public Task<List<DataSetStorageUsageSnapshot>> GetHistoryAsync(
        Guid tenantId, int take = 168, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        return dataSetDb.StorageUsageSnapshots.AsNoTracking().Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.Id).Take(Math.Clamp(take, 1, 744)).ToListAsync(cancellationToken);
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
