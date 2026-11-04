using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;
using EDAN.Tensor.SharedKernel.Time;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>접근 정책 생성 시 Tenant와 하위 소유권 경계를 검증합니다.</summary>
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

    private static Result<AuthorizationPolicyId> Fail(string code, string description) =>
        Result<AuthorizationPolicyId>.Failure(new(code, description));
}
