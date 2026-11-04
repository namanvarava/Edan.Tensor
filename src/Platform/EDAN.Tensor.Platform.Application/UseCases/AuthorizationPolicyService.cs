using System.Text.Json;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>접근 정책의 생성·조회·변경과 변경 감사 기록을 관리합니다.</summary>
public sealed class AuthorizationPolicyService(
    ITenantRepository tenants,
    IOrganizationRepository organizations,
    IProjectRepository projects,
    IAuthorizationPolicyRepository policies,
    IPlatformUnitOfWork unitOfWork,
    ISystemClock clock)
{
    /// <summary>검증된 Tenant·Organization·Project 범위에 접근 정책을 등록합니다.</summary>
    public async Task<Result<AuthorizationPolicyId>> CreateAsync(TenantId tenantId, OrganizationId? organizationId,
        ProjectId? projectId, string subjectId, string action, string resourceType,
        AuthorizationPolicyEffect effect, int priority, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.Status != TenantStatus.Active)
                return Fail("platform.tenant.unavailable", "활성 Tenant에서만 접근 정책을 등록할 수 있습니다.");

            if (organizationId is not null)
            {
                var organization = await organizations.GetByIdAsync(organizationId, cancellationToken);
                if (organization is null || organization.Status != OrganizationStatus.Active || organization.TenantId != tenantId)
                    return Fail("platform.organization.scope_mismatch", "조직이 요청한 Tenant에 속하지 않거나 활성 상태가 아닙니다.");
            }

            if (projectId is not null)
            {
                var project = await projects.GetByIdAsync(projectId, cancellationToken);
                if (project is null || project.Status != ProjectStatus.Active ||
                    project.TenantId != tenantId || project.OrganizationId != organizationId)
                    return Fail("platform.project.scope_mismatch", "프로젝트의 Tenant·Organization 소속이 요청 범위와 일치하지 않습니다.");
            }

            var policy = AuthorizationPolicy.Create(new AuthorizationPolicyId(Guid.NewGuid()), tenantId,
                organizationId, projectId, subjectId, action, resourceType, effect, priority, clock.UtcNow);
            policies.Add(policy);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthorizationPolicyId>.Success(policy.Id);
        }
        catch (ArgumentException ex)
        {
            return Fail("platform.validation", ex.Message);
        }
    }

    /// <summary>선택 Tenant에 속한 접근 정책을 생성 시각 역순으로 조회합니다.</summary>
    public Task<IReadOnlyList<AuthorizationPolicy>> GetByTenantAsync(TenantId tenantId,
        CancellationToken cancellationToken = default) => policies.GetByTenantAsync(tenantId, cancellationToken);

    /// <summary>정책 효과와 우선순위를 변경하고 변경 전후 상태를 감사 기록으로 남깁니다.</summary>
    public Task<Result<AuthorizationPolicyId>> UpdateAsync(TenantId tenantId, AuthorizationPolicyId policyId,
        AuthorizationPolicyEffect effect, int priority, string actorSubjectId,
        CancellationToken cancellationToken = default) =>
        ChangeAsync(tenantId, policyId, actorSubjectId, "Updated", policy =>
        {
            policy.Update(effect, priority);
        }, cancellationToken);

    /// <summary>정책 활성 상태를 변경하고 변경 전후 상태를 감사 기록으로 남깁니다.</summary>
    public Task<Result<AuthorizationPolicyId>> SetEnabledAsync(TenantId tenantId, AuthorizationPolicyId policyId,
        bool enabled, string actorSubjectId, CancellationToken cancellationToken = default) =>
        ChangeAsync(tenantId, policyId, actorSubjectId, enabled ? "Enabled" : "Disabled",
            policy => policy.SetEnabled(enabled), cancellationToken);

    private async Task<Result<AuthorizationPolicyId>> ChangeAsync(TenantId tenantId, AuthorizationPolicyId policyId,
        string actorSubjectId, string changeType, Action<AuthorizationPolicy> change,
        CancellationToken cancellationToken)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(actorSubjectId);
            var policy = await policies.GetByIdAsync(policyId, cancellationToken);
            if (policy is null || policy.TenantId != tenantId)
                return Fail("platform.authorization_policy.not_found", "선택한 Tenant에서 접근 정책을 찾을 수 없습니다.");

            var before = JsonSerializer.Serialize(new
            {
                Effect = policy.Effect.ToString(),
                policy.Priority,
                policy.IsEnabled
            });
            change(policy);
            var after = JsonSerializer.Serialize(new
            {
                Effect = policy.Effect.ToString(),
                policy.Priority,
                policy.IsEnabled
            });
            policies.AddChangeAudit(AuthorizationPolicyChangeAuditEntry.Create(
                new AuthorizationPolicyChangeAuditEntryId(Guid.NewGuid()), tenantId, policy.Id.Value,
                actorSubjectId, changeType, before, after, clock.UtcNow));
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<AuthorizationPolicyId>.Success(policy.Id);
        }
        catch (ArgumentException ex)
        {
            return Fail("platform.validation", ex.Message);
        }
    }

    private static Result<AuthorizationPolicyId> Fail(string code, string description) =>
        Result<AuthorizationPolicyId>.Failure(new(code, description));
}