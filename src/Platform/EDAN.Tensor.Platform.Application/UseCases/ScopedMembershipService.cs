using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Exceptions;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>조직·프로젝트 소속을 상위 소유권과 대조하여 등록합니다.</summary>
public sealed class ScopedMembershipService(
    ITenantRepository tenants,
    IOrganizationRepository organizations,
    IProjectRepository projects,
    ITenantMembershipRepository tenantMemberships,
    IScopedMembershipRepository memberships,
    IPlatformUnitOfWork unitOfWork)
{
    /// <summary>활성 조직에 인증 주체의 조직 역할을 부여합니다.</summary>
    public async Task<Result<OrganizationMembershipId>> GrantOrganizationAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, OrganizationRole role, CancellationToken cancellationToken = default)
    {
        if (!await tenantMemberships.HasActiveMembershipAsync(tenantId, subjectId, cancellationToken))
            return Fail<OrganizationMembershipId>("platform.membership.tenant_required", "대상 사용자는 해당 Tenant의 활성 구성원이어야 합니다.");

        var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null || tenant.Status != TenantStatus.Active)
            return Fail<OrganizationMembershipId>("platform.tenant.unavailable", "활성 Tenant에서만 조직 소속을 부여할 수 있습니다.");

        var organization = await organizations.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null || organization.TenantId != tenantId || organization.Status != OrganizationStatus.Active)
            return Fail<OrganizationMembershipId>("platform.organization.scope_mismatch", "조직이 선택한 Tenant에 속하지 않거나 활성 상태가 아닙니다.");
        try
        {
            if (await memberships.OrganizationMembershipExistsAsync(tenantId, organizationId, subjectId, cancellationToken))
                return Fail<OrganizationMembershipId>("platform.organization_membership.conflict", "해당 사용자는 이미 조직 구성원입니다.");
            var membership = OrganizationMembership.Grant(new(Guid.NewGuid()), tenantId, organizationId, subjectId, role);
            memberships.Add(membership);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<OrganizationMembershipId>.Success(membership.Id);
        }
        catch (ArgumentException ex) { return Fail<OrganizationMembershipId>("platform.validation", ex.Message); }
    }

    /// <summary>활성 프로젝트에 인증 주체의 프로젝트 역할을 부여합니다.</summary>
    public async Task<Result<ProjectMembershipId>> GrantProjectAsync(TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string subjectId, ProjectRole role, CancellationToken cancellationToken = default)
    {
        if (!await tenantMemberships.HasActiveMembershipAsync(tenantId, subjectId, cancellationToken))
            return Fail<ProjectMembershipId>("platform.membership.tenant_required", "대상 사용자는 해당 Tenant의 활성 구성원이어야 합니다.");

        var tenant = await tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant is null || tenant.Status != TenantStatus.Active)
            return Fail<ProjectMembershipId>("platform.tenant.unavailable", "활성 Tenant에서만 프로젝트 소속을 부여할 수 있습니다.");

        var project = await projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null || project.TenantId != tenantId || project.OrganizationId != organizationId || project.Status != ProjectStatus.Active)
            return Fail<ProjectMembershipId>("platform.project.scope_mismatch", "프로젝트의 Tenant·조직 소속이 일치하지 않거나 활성 상태가 아닙니다.");
        try
        {
            if (await memberships.ProjectMembershipExistsAsync(tenantId, projectId, subjectId, cancellationToken))
                return Fail<ProjectMembershipId>("platform.project_membership.conflict", "해당 사용자는 이미 프로젝트 구성원입니다.");
            var membership = ProjectMembership.Grant(new(Guid.NewGuid()), tenantId, organizationId, projectId, subjectId, role);
            memberships.Add(membership);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<ProjectMembershipId>.Success(membership.Id);
        }
        catch (ArgumentException ex) { return Fail<ProjectMembershipId>("platform.validation", ex.Message); }
    }

    /// <summary>조직 구성원의 역할을 변경하거나 접근 권한을 회수합니다.</summary>
    public async Task<Result> UpdateOrganizationAsync(
        TenantId tenantId, OrganizationId organizationId, OrganizationMembershipId membershipId,
        OrganizationRole? role, bool revoke, CancellationToken cancellationToken = default)
    {
        var membership = await memberships.GetOrganizationMembershipAsync(tenantId, organizationId, membershipId, cancellationToken);
        if (membership is null)
            return Result.Failure(new Error("platform.organization_membership.not_found", "조직 멤버십을 찾을 수 없습니다."));
        if (!revoke && !role.HasValue)
            return Result.Failure(new Error("platform.validation", "Role 또는 Revoke 중 하나가 필요합니다."));
        try
        {
            if (revoke) membership.Revoke();
            else membership.ChangeRole(role!.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }

    /// <summary>프로젝트 구성원의 역할을 변경하거나 접근 권한을 회수합니다.</summary>
    public async Task<Result> UpdateProjectAsync(
        TenantId tenantId, OrganizationId organizationId, ProjectId projectId, ProjectMembershipId membershipId,
        ProjectRole? role, bool revoke, CancellationToken cancellationToken = default)
    {
        var membership = await memberships.GetProjectMembershipAsync(tenantId, organizationId, projectId, membershipId, cancellationToken);
        if (membership is null)
            return Result.Failure(new Error("platform.project_membership.not_found", "프로젝트 멤버십을 찾을 수 없습니다."));
        if (!revoke && !role.HasValue)
            return Result.Failure(new Error("platform.validation", "Role 또는 Revoke 중 하나가 필요합니다."));
        try
        {
            if (revoke) membership.Revoke();
            else membership.ChangeRole(role!.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex) { return Result.Failure(new Error(ex.Code, ex.Message)); }
        catch (ArgumentException ex) { return Result.Failure(new Error("platform.validation", ex.Message)); }
    }

    private static Result<T> Fail<T>(string code, string description) => Result<T>.Failure(new Error(code, description));
}
