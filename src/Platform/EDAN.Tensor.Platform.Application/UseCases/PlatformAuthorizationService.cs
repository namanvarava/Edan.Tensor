using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.UseCases;

/// <summary>데이터베이스 소속과 역할을 이용해 범위별 쓰기 권한을 평가합니다.</summary>
public sealed class PlatformAuthorizationService(
    ITenantMembershipRepository tenantMemberships,
    IScopedMembershipRepository scopedMemberships)
{
    /// <summary>Tenant·Organization·Project 범위에서 자원 및 정책 생성 권한을 확인합니다.</summary>
    public async Task<bool> CanCreateInScopeAsync(TenantId tenantId, OrganizationId? organizationId, ProjectId? projectId, string subjectId, CancellationToken cancellationToken = default)
    {
        var tenantMembership = await tenantMemberships.GetAsync(tenantId, subjectId, cancellationToken);
        if (tenantMembership is null || tenantMembership.Status != TenantMembershipStatus.Active)
            return false;

        if (tenantMembership.Role == TenantRole.Administrator)
            return true;

        if (organizationId is null)
            return false;

        if (projectId is null)
            return await scopedMemberships.HasOrganizationWriteAccessAsync(tenantId, organizationId, subjectId, cancellationToken);

        return await scopedMemberships.HasProjectWriteAccessAsync(tenantId, organizationId, projectId, subjectId, cancellationToken)
            || await scopedMemberships.HasOrganizationManagementAccessAsync(tenantId, organizationId, subjectId, cancellationToken);
    }
}
