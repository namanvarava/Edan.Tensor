using System.Text.Json;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>테넌트별 감사 보존 정책을 변경하고 변경 이력을 같은 저장 작업으로 기록합니다.</summary>
public sealed class AuthorizationAuditRetentionService(
    IAuthorizationAuditRetentionRepository policies,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>저장된 정책을 조회하고 없으면 기본 365일 정책을 반환합니다.</summary>
    public async Task<AuthorizationAuditRetentionPolicyView> GetAsync(TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        var policy = await policies.GetByTenantAsync(tenantId, cancellationToken);
        return policy is null
            ? new AuthorizationAuditRetentionPolicyView(tenantId.Value, 365, false, null, null, null, 0)
            : ToView(policy);
    }

    /// <summary>보존 정책을 생성하거나 갱신하고 변경 전후 상태를 기록합니다.</summary>
    public async Task<AuthorizationAuditRetentionPolicyView> UpdateAsync(TenantId tenantId,
        int retentionDays, bool isLegalHold, string? legalHoldReason, string actor,
        CancellationToken cancellationToken = default)
    {
        var policy = await policies.GetByTenantAsync(tenantId, cancellationToken);
        var before = policy is null ? null : ToSnapshot(policy);
        var now = clock.UtcNow;

        if (policy is null)
        {
            policy = AuthorizationAuditRetentionPolicy.Create(tenantId, retentionDays,
                isLegalHold, legalHoldReason, actor, now);
            policies.Add(policy);
        }
        else
        {
            policy.Update(retentionDays, isLegalHold, legalHoldReason, actor, now);
        }

        var after = ToSnapshot(policy);
        var changeType = before is null ? "Created"
            : before.IsLegalHold != after.IsLegalHold
                ? (after.IsLegalHold ? "LegalHoldEnabled" : "LegalHoldReleased")
                : "Updated";
        policies.AddChange(AuthorizationAuditRetentionChange.Create(policy.Id, tenantId,
            actor, changeType, JsonSerializer.Serialize(before, JsonOptions),
            JsonSerializer.Serialize(after, JsonOptions), now));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToView(policy);
    }

    private static AuthorizationAuditRetentionPolicyView ToView(AuthorizationAuditRetentionPolicy x) =>
        new(x.TenantId.Value, x.RetentionDays, x.IsLegalHold, x.LegalHoldReason,
            x.UpdatedBy, x.UpdatedAtUtc, x.Version);

    private static RetentionSnapshot ToSnapshot(AuthorizationAuditRetentionPolicy x) =>
        new(x.RetentionDays, x.IsLegalHold, x.LegalHoldReason, x.UpdatedBy, x.UpdatedAtUtc, x.Version);

    private sealed record RetentionSnapshot(int RetentionDays, bool IsLegalHold,
        string? LegalHoldReason, string UpdatedBy, DateTimeOffset UpdatedAtUtc, long Version);
}

/// <summary>감사 보존 정책 조회 결과를 제공합니다.</summary>
public sealed record AuthorizationAuditRetentionPolicyView(Guid TenantId, int RetentionDays,
    bool IsLegalHold, string? LegalHoldReason, string? UpdatedBy, DateTimeOffset? UpdatedAtUtc, long Version);
