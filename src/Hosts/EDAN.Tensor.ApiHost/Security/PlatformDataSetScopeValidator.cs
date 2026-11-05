using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>DataSet 소유 범위가 동일 Tenant의 활성 Organization·Project 계층에 속하는지 확인합니다.</summary>
public sealed class PlatformDataSetScopeValidator(PlatformDbContext db) : IDataSetScopeValidator
{
    public async Task<bool> IsValidAsync(DataSetScope scope, CancellationToken cancellationToken = default)
    {
        var tenantId = new TenantId(scope.TenantId);
        var organizationId = new OrganizationId(scope.OrganizationId);
        var projectId = new ProjectId(scope.ProjectId);

        var tenantIsActive = await db.Tenants.AnyAsync(
            tenant => tenant.Id == tenantId && tenant.Status == TenantStatus.Active, cancellationToken);
        if (!tenantIsActive) return false;

        var organizationIsActive = await db.Organizations.AnyAsync(
            organization => organization.Id == organizationId
                && organization.TenantId == tenantId
                && organization.Status == OrganizationStatus.Active, cancellationToken);
        if (!organizationIsActive) return false;

        return await db.Projects.AnyAsync(
            project => project.Id == projectId
                && project.TenantId == tenantId
                && project.OrganizationId == organizationId
                && project.Status == ProjectStatus.Active, cancellationToken);
    }
}
