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
        // WorkerTask 경로에서는 단일 Tenant만 조회해 다른 Tenant의 메타데이터를 메모리에 적재하지 않습니다.
        // 운영/관리용 전체 수집(CaptureDueAsync)은 기존처럼 전체 범위를 처리합니다.
        var dataSetQuery = dataSetDb.DataSets.AsNoTracking();
        var policyQuery = platformDb.StorageQuotaPolicies.AsNoTracking();
        var activeEventQuery = dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.EventType == "Warning" || x.EventType == "Exceeded");

        if (onlyTenantId.HasValue)
        {
            var tenantId = onlyTenantId.Value;
            dataSetQuery = dataSetQuery.Where(x => x.TenantId == tenantId);
            policyQuery = policyQuery.Where(x => x.TenantId.Value == tenantId);
            activeEventQuery = activeEventQuery.Where(x => x.TenantId == tenantId);
        }

        var dataSets = await dataSetQuery
            .OrderBy(x => x.TenantId).ThenBy(x => x.OrganizationId).ThenBy(x => x.ProjectId)
            .ToListAsync(cancellationToken);
        var policies = await policyQuery.ToListAsync(cancellationToken);

        // 데이터가 아직 없는 Tenant도 활성 정책이 있으면 평가 대상에 포함합니다.
        // 삭제된 정책도 마지막 활성 이벤트가 남아 있으면 해제 전이를 기록할 수 있도록 이력을 평가 대상에 포함합니다.
        var eventTenantIds = await activeEventQuery
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
                        x.TenantId == tenantId && x.OrganizationId == policy.TargetId),
                    StorageQuotaPolicyScope.Project => dataSets.FirstOrDefault(x =>
                        x.TenantId == tenantId && x.ProjectId == policy.TargetId),
                    StorageQuotaPolicyScope.DataSet => dataSets.FirstOrDefault(x =>
                        x.TenantId == tenantId && x.Id.Value == policy.TargetId),
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

    /// <summary>Tenant의 저장량 스냅샷을 최신 순서로 제한된 개수만 조회합니다.</summary>
    /// <param name="tenantId">조회할 Tenant 식별자입니다.</param>
    /// <param name="take">반환할 스냅샷 개수이며 1 이상 744 이하이어야 합니다.</param>
    /// <param name="cancellationToken">저장소 조회를 취소하는 토큰입니다.</param>
    /// <returns>Tenant에 속한 저장량 스냅샷 목록을 최신 순서로 반환합니다.</returns>
    /// <exception cref="ArgumentException">Tenant 식별자가 비어 있으면 예외를 반환합니다.</exception>
    /// <exception cref="ArgumentOutOfRangeException">조회 개수가 1~744 범위를 벗어나면 예외를 반환합니다.</exception>
    public Task<List<DataSetStorageUsageSnapshot>> GetHistoryAsync(
        Guid tenantId, int take = 168, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        ValidateTake(take, 744);
        return dataSetDb.StorageUsageSnapshots.AsNoTracking().Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.Id).Take(take).ToListAsync(cancellationToken);
    }

    /// <summary>Tenant의 quota 임계치 이벤트를 최신 순서로 제한된 개수만 조회합니다.</summary>
    /// <param name="tenantId">조회할 Tenant 식별자입니다.</param>
    /// <param name="take">반환할 이벤트 개수이며 1 이상 500 이하이어야 합니다.</param>
    /// <param name="cancellationToken">저장소 조회를 취소하는 토큰입니다.</param>
    /// <returns>Tenant에 속한 quota 임계치 이벤트 목록을 최신 순서로 반환합니다.</returns>
    /// <exception cref="ArgumentException">Tenant 식별자가 비어 있으면 예외를 반환합니다.</exception>
    /// <exception cref="ArgumentOutOfRangeException">조회 개수가 1~500 범위를 벗어나면 예외를 반환합니다.</exception>
    public Task<List<DataSetQuotaThresholdEvent>> GetQuotaEventHistoryAsync(
        Guid tenantId, int take = 100, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        ValidateTake(take, 500);
        return dataSetDb.QuotaThresholdEvents.AsNoTracking().Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.OccurredAtUtc).ThenByDescending(x => x.Id)
            .Take(take).ToListAsync(cancellationToken);
    }

    private static void ValidateTake(int take, int maximum)
    {
        if (take < 1 || take > maximum)
            throw new ArgumentOutOfRangeException(nameof(take), take,
                $"조회 개수는 1 이상 {maximum} 이하이어야 합니다.");
    }
}
