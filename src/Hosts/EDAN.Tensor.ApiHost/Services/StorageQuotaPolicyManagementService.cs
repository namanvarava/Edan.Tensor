using System.Text.Json;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>Tenant 경계를 지키며 Quota 정책과 변경 감사 기록을 원자적으로 관리합니다.</summary>
public sealed class StorageQuotaPolicyManagementService(PlatformDbContext db)
{
    public async Task<IReadOnlyList<StorageQuotaPolicy>> GetPoliciesAsync(TenantId tenantId, CancellationToken token) =>
        await db.StorageQuotaPolicies.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Scope).ThenBy(x => x.TargetId)
            .ToListAsync(token);

    public async Task<IReadOnlyList<StorageQuotaPolicyChangeAuditEntry>> GetHistoryAsync(
        TenantId tenantId, Guid policyId, CancellationToken token)
    {
        var belongs = await db.StorageQuotaPolicies.AnyAsync(x => x.TenantId == tenantId && x.Id == policyId, token);
        if (!belongs) return [];
        return await db.StorageQuotaPolicyChangeAuditEntries.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PolicyId == policyId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync(token);
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
        policy.Update(tenantLimit, organizationLimit, projectLimit, dataSetLimit,
            warningPercent, planCode, actor, expectedVersion, now);
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
        policy.SetEnabled(enabled, actor, expectedVersion, now);
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
            StorageQuotaPolicyScope.DataSet => true,
            _ => false
        };
        if (!valid) throw new ArgumentException("정책 대상이 지정한 Tenant에 속하지 않거나 범위가 올바르지 않습니다.");
    }

    private static string Snapshot(StorageQuotaPolicy x) => JsonSerializer.Serialize(new
    {
        x.Id, TenantId = x.TenantId.Value, Scope = x.Scope.ToString(), x.TargetId,
        x.TenantLimitBytes, x.OrganizationLimitBytes, x.ProjectLimitBytes, x.DataSetLimitBytes,
        x.WarningThresholdPercent, x.PlanCode, x.IsEnabled, x.Version, x.UpdatedBy, x.UpdatedAtUtc
    });
}
