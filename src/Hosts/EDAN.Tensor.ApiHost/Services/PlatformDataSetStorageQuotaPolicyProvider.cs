using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.UseCases;
using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>Platform에 저장된 Tenant 계층 정책을 DataSet quota 계약으로 변환합니다.</summary>
public sealed class PlatformDataSetStorageQuotaPolicyProvider(
    PlatformDbContext platformDb,
    IConfiguration configuration) : IDataSetStorageQuotaPolicyProvider
{
    public async Task<DataSetStorageQuotaPolicy> GetPolicyAsync(
        DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        var configured = new DataSetStorageQuotaPolicy(
            configuration.GetValue<long?>("DataSetContent:Quota:TenantLimitBytes"),
            configuration.GetValue<long?>("DataSetContent:Quota:OrganizationLimitBytes"),
            configuration.GetValue<long?>("DataSetContent:Quota:ProjectLimitBytes"),
            configuration.GetValue<long?>("DataSetContent:Quota:DataSetLimitBytes"),
            configuration.GetValue<int?>("DataSetContent:Quota:WarningThresholdPercent") ?? 80);
        var stored = await platformDb.StorageQuotaPolicies.AsNoTracking()
            .Where(x => x.TenantId == new TenantId(allocation.Scope.TenantId) && x.IsEnabled)
            .ToListAsync(cancellationToken);
        var assignments = stored.Select(x => new DataSetStorageQuotaPolicyAssignment(
            (DataSetStorageQuotaScope)Enum.Parse<StorageQuotaPolicyScope>(x.Scope.ToString()),
            x.TargetId,
            new DataSetStorageQuotaPolicy(x.TenantLimitBytes, x.OrganizationLimitBytes,
                x.ProjectLimitBytes, x.DataSetLimitBytes, x.WarningThresholdPercent)));
        return await new HierarchicalDataSetStorageQuotaPolicyProvider(configured, assignments)
            .GetPolicyAsync(allocation, cancellationToken);
    }
}
