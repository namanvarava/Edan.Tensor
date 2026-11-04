using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>조직·프로젝트 소속의 중복 검사와 권한 조회를 제공합니다.</summary>
public interface IScopedMembershipRepository
{
    Task<bool> OrganizationMembershipExistsAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default);
    Task<bool> ProjectMembershipExistsAsync(TenantId tenantId, ProjectId projectId, string subjectId, CancellationToken cancellationToken = default);
    Task<bool> HasOrganizationWriteAccessAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default);
    Task<bool> HasOrganizationManagementAccessAsync(TenantId tenantId, OrganizationId organizationId, string subjectId, CancellationToken cancellationToken = default);
    Task<bool> HasProjectWriteAccessAsync(TenantId tenantId, OrganizationId organizationId, ProjectId projectId, string subjectId, CancellationToken cancellationToken = default);
    void Add(OrganizationMembership membership);
    void Add(ProjectMembership membership);
}
