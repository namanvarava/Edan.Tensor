using System.Text.Json;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>Tenant 경계를 지키며 Quota 정책과 변경 감사 기록을 원자적으로 관리합니다.</summary>
public sealed class StorageQuotaPolicyManagementService(PlatformDbContext db, DataSetDbContext dataSetDb)
{
    /// <summary>선택한 Tenant의 저장 용량 정책을 제한된 페이지로 조회합니다.</summary>
    /// <param name="tenantId">정책을 소유한 Tenant 식별자입니다.</param>
    /// <param name="page">1부터 시작하는 페이지 번호입니다.</param>
    /// <param name="pageSize">페이지에 포함할 정책 수이며 1~100 사이여야 합니다.</param>
    /// <param name="token">작업 취소를 감시하는 토큰입니다.</param>
    /// <returns>Tenant 범위로 제한된 저장 용량 정책 페이지를 반환합니다.</returns>
    public async Task<StorageQuotaPolicyPage> GetPoliciesAsync(TenantId tenantId, int page = 1,
        int pageSize = 50, CancellationToken token = default)
    {
        var skip = ValidatePage(page, pageSize);
        var query = db.StorageQuotaPolicies.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Scope).ThenBy(x => x.TargetId);
        var totalCount = await query.CountAsync(token);
        var items = await query.Skip(skip).Take(pageSize).ToListAsync(token);
        return new StorageQuotaPolicyPage(items, totalCount, page, pageSize);
    }

    /// <summary>선택한 Tenant 정책의 변경 감사 이력을 제한된 페이지로 조회합니다.</summary>
    /// <param name="tenantId">정책을 소유한 Tenant 식별자입니다.</param>
    /// <param name="policyId">변경 이력을 조회할 정책 식별자입니다.</param>
    /// <param name="page">1부터 시작하는 페이지 번호입니다.</param>
    /// <param name="pageSize">페이지에 포함할 이력 수이며 1~100 사이여야 합니다.</param>
    /// <param name="token">작업 취소를 감시하는 토큰입니다.</param>
    /// <returns>정책이 Tenant에 속하면 변경 이력 페이지를, 그렇지 않으면 null을 반환합니다.</returns>
    public async Task<StorageQuotaPolicyHistoryPage?> GetHistoryAsync(
        TenantId tenantId, Guid policyId, int page = 1, int pageSize = 50, CancellationToken token = default)
    {
        var skip = ValidatePage(page, pageSize);
        var belongs = await db.StorageQuotaPolicies.AnyAsync(x => x.TenantId == tenantId && x.Id == policyId, token);
        if (!belongs) return null;
        var query = db.StorageQuotaPolicyChangeAuditEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PolicyId == policyId)
            .Select(x => x);
        var totalCount = await query.CountAsync(token);
        // SQLite 공급자는 DateTimeOffset 정렬을 SQL로 변환하지 못하므로 UTC 시각으로 메모리 정렬합니다.
        var items = (await query.ToListAsync(token))
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToArray();
        return new StorageQuotaPolicyHistoryPage(items, totalCount, page, pageSize);
    }

    public async Task<StorageQuotaPolicy> CreateAsync(TenantId tenantId, StorageQuotaPolicyScope scope,
        Guid targetId, long? tenantLimit, long? organizationLimit, long? projectLimit, long? dataSetLimit,
        int warningPercent, string planCode, string actor, CancellationToken token)
    {
        await ValidateTargetAsync(tenantId, scope, targetId, token);
        if (await db.StorageQuotaPolicies.AnyAsync(x => x.TenantId == tenantId && x.Scope == scope && x.TargetId == targetId, token))
            throw new InvalidOperationException("quota_policy.conflict");
        var now = DateTimeOffset.UtcNow;
        var policy = StorageQuotaPolicy.Create(Guid.NewGuid(), tenantId, scope, targetId,
            tenantLimit, organizationLimit, projectLimit, dataSetLimit, warningPercent, planCode, actor, now);
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        db.StorageQuotaPolicies.Add(policy);
        db.StorageQuotaPolicyChangeAuditEntries.Add(StorageQuotaPolicyChangeAuditEntry.Create(
            Guid.NewGuid(), tenantId, policy.Id, actor, "Created", "{}", Snapshot(policy), now));
        await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return policy;
    }

    public async Task<StorageQuotaPolicy?> UpdateAsync(TenantId tenantId, Guid policyId, long expectedVersion,
        long? tenantLimit, long? organizationLimit, long? projectLimit, long? dataSetLimit,
        int warningPercent, string planCode, string actor, CancellationToken token)
    {
        var policy = await db.StorageQuotaPolicies.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == policyId, token);
        if (policy is null) return null;
        var before = Snapshot(policy);
        var now = DateTimeOffset.UtcNow;
        try
        {
            policy.Update(tenantLimit, organizationLimit, projectLimit,
                dataSetLimit, warningPercent, planCode, actor, expectedVersion, now);
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("버전", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("quota_policy.version_conflict", exception);
        }
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        db.StorageQuotaPolicyChangeAuditEntries.Add(StorageQuotaPolicyChangeAuditEntry.Create(
            Guid.NewGuid(), tenantId, policy.Id, actor, "Updated", before, Snapshot(policy), now));
        try
        {
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return policy;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(token);
            throw new InvalidOperationException("quota_policy.version_conflict");
        }
    }

    public async Task<StorageQuotaPolicy?> SetEnabledAsync(TenantId tenantId, Guid policyId,
        bool enabled, long expectedVersion, string actor, CancellationToken token)
    {
        var policy = await db.StorageQuotaPolicies.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == policyId, token);
        if (policy is null) return null;
        var before = Snapshot(policy);
        var now = DateTimeOffset.UtcNow;
        try
        {
            policy.SetEnabled(enabled, actor, expectedVersion, now);
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains("버전", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("quota_policy.version_conflict", exception);
        }
        if (before == Snapshot(policy)) return policy;
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        db.StorageQuotaPolicyChangeAuditEntries.Add(StorageQuotaPolicyChangeAuditEntry.Create(
            Guid.NewGuid(), tenantId, policy.Id, actor, enabled ? "Enabled" : "Disabled",
            before, Snapshot(policy), now));
        try
        {
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return policy;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(token);
            throw new InvalidOperationException("quota_policy.version_conflict");
        }
    }

    private async Task ValidateTargetAsync(TenantId tenantId, StorageQuotaPolicyScope scope,
        Guid targetId, CancellationToken token)
    {
        if (targetId == Guid.Empty) throw new ArgumentException("정책 대상 ID는 비어 있을 수 없습니다.");
        var valid = scope switch
        {
            StorageQuotaPolicyScope.Tenant => targetId == tenantId.Value,
            StorageQuotaPolicyScope.Organization => await db.Organizations.AnyAsync(
                x => x.TenantId == tenantId && x.Id == new OrganizationId(targetId), token),
            StorageQuotaPolicyScope.Project => await db.Projects.AnyAsync(
                x => x.TenantId == tenantId && x.Id == new ProjectId(targetId), token),
            StorageQuotaPolicyScope.DataSet => await dataSetDb.DataSets.AnyAsync(
                x => x.Id == new EDAN.Tensor.DataSet.Domain.Model.DataSetId(targetId)
                    && x.TenantId == tenantId.Value, token),
            _ => false
        };
        if (!valid) throw new ArgumentException("정책 대상이 지정한 Tenant에 속하지 않거나 범위가 올바르지 않습니다.");
    }

    private static int ValidatePage(int page, int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page), "페이지 번호는 1 이상이어야 합니다.");
        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "페이지 크기는 1~100이어야 합니다.");
        try
        {
            return checked((page - 1) * pageSize);
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "페이지 범위가 너무 큽니다.");
        }
    }

    private static string Snapshot(StorageQuotaPolicy x) => JsonSerializer.Serialize(new
    {
        x.Id, TenantId = x.TenantId.Value, Scope = x.Scope.ToString(), x.TargetId,
        x.TenantLimitBytes, x.OrganizationLimitBytes, x.ProjectLimitBytes, x.DataSetLimitBytes,
        x.WarningThresholdPercent, x.PlanCode, x.IsEnabled, x.Version, x.UpdatedBy, x.UpdatedAtUtc
    });
}

/// <summary>저장 용량 정책의 페이지 조회 결과를 보관합니다.</summary>
public sealed record StorageQuotaPolicyPage(
    IReadOnlyList<StorageQuotaPolicy> Items, int TotalCount, int Page, int PageSize);

/// <summary>저장 용량 정책 변경 이력의 페이지 조회 결과를 보관합니다.</summary>
public sealed record StorageQuotaPolicyHistoryPage(
    IReadOnlyList<StorageQuotaPolicyChangeAuditEntry> Items, int TotalCount, int Page, int PageSize);
