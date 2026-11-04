using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>역할 기반 권한과 계층형 접근 정책을 함께 평가하고 모든 결과를 감사 기록으로 남깁니다.</summary>
public sealed class PlatformAuthorizationService(
    ITenantMembershipRepository tenantMemberships,
    IScopedMembershipRepository scopedMemberships,
    IAuthorizationPolicyRepository policies,
    IAuthorizationAuditRepository audits,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock)
{
    /// <summary>Tenant·Organization·Project 범위에서 자원 및 정책 생성 권한을 평가합니다.</summary>
    public async Task<bool> CanCreateInScopeAsync(TenantId tenantId, OrganizationId? organizationId,
        ProjectId? projectId, string subjectId, CancellationToken cancellationToken = default, string action = "resource:create", string resourceType = "*")
    {
        var decision = await EvaluateAsync(tenantId, organizationId, projectId, subjectId,
            action, resourceType, cancellationToken);
        return decision.IsAllowed;
    }

    /// <summary>역할 권한을 기본 허용으로 삼되 적용 가능한 명시적 정책 거부를 항상 우선합니다.</summary>
    public async Task<AuthorizationDecision> EvaluateAsync(TenantId tenantId, OrganizationId? organizationId,
        ProjectId? projectId, string subjectId, string action, string resourceType,
        CancellationToken cancellationToken = default)
    {
        var membership = await tenantMemberships.GetAsync(tenantId, subjectId, cancellationToken);
        var roleAllowed = false;
        if (membership is not null && membership.Status == TenantMembershipStatus.Active)
        {
            if (membership.Role == TenantRole.Administrator)
                roleAllowed = true;
            else if (organizationId is not null && projectId is null)
                roleAllowed = await scopedMemberships.HasOrganizationWriteAccessAsync(tenantId, organizationId, subjectId, cancellationToken);
            else if (organizationId is not null && projectId is not null)
                roleAllowed = await scopedMemberships.HasProjectWriteAccessAsync(tenantId, organizationId, projectId, subjectId, cancellationToken)
                    || await scopedMemberships.HasOrganizationManagementAccessAsync(tenantId, organizationId, subjectId, cancellationToken);
        }

        var applicable = await policies.GetApplicableAsync(tenantId, organizationId, projectId,
            subjectId, action, resourceType, cancellationToken);
        var deny = applicable.Where(x => x.IsEnabled && x.Effect == AuthorizationPolicyEffect.Deny)
            .OrderByDescending(x => x.Priority).FirstOrDefault();
        var allow = applicable.Where(x => x.IsEnabled && x.Effect == AuthorizationPolicyEffect.Allow)
            .OrderByDescending(x => x.Priority).FirstOrDefault();

        AuthorizationDecision decision;
        if (deny is not null)
            decision = new(false, "policy.explicit_deny", deny.Id.Value.ToString());
        else if (allow is not null)
            decision = new(true, "policy.explicit_allow", allow.Id.Value.ToString());
        else if (roleAllowed)
            decision = new(true, "role.grant", null);
        else
            decision = new(false, "authorization.default_deny", null);

        var scope = $"tenant:{tenantId.Value};organization:{organizationId?.Value.ToString() ?? "-"};project:{projectId?.Value.ToString() ?? "-"}";
        audits.Add(AuthorizationAuditEntry.Create(new AuthorizationAuditEntryId(Guid.NewGuid()), tenantId,
            subjectId, action, resourceType, scope, decision, clock.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return decision;
    }
}
