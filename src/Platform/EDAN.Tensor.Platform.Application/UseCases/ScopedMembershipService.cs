using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>조직·프로젝트 소속을 상위 소유권과 대조하여 등록합니다.</summary>
public sealed class ScopedMembershipService(
    IOrganizationRepository organizations,
    IProjectRepository projects,
    IScopedMembershipRepository memberships,
    IPlatformUnitOfWork unitOfWork)
{
    /// <summary>활성 조직에 인증 주체의 조직 역할을 부여합니다.</summary>
    public async Task<Result<OrganizationMembershipId>> GrantOrganizationAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, OrganizationRole role, CancellationToken cancellationToken = default)
    {
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

    private static Result<T> Fail<T>(string code, string description) => Result<T>.Failure(new Error(code, description));
}
