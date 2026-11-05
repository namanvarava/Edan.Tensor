using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.UseCases;
using EDAN.Tensor.DataSet.Domain.Model;
using Xunit;

namespace EDAN.Tensor.DataSet.Application.Tests;

public sealed class HierarchicalDataSetStorageQuotaPolicyProviderTests
{
    [Fact]
    public async Task ResolvesTenantOrganizationProjectAndDataSetPoliciesInOrder()
    {
        var tenant = Guid.NewGuid();
        var organization = Guid.NewGuid();
        var project = Guid.NewGuid();
        var dataSet = new DataSetId(Guid.NewGuid());
        var provider = new HierarchicalDataSetStorageQuotaPolicyProvider(
            new DataSetStorageQuotaPolicy(1_000, 900, 800, 700, 80),
            [
                Assignment(DataSetStorageQuotaScope.Tenant, tenant, new(900, null, null, null, 85)),
                Assignment(DataSetStorageQuotaScope.Organization, organization, new(null, 700, null, null, 75)),
                Assignment(DataSetStorageQuotaScope.Project, project, new(null, null, 500, null, 70)),
                Assignment(DataSetStorageQuotaScope.DataSet, dataSet.Value, new(null, null, null, 300, 65))
            ]);

        var result = await provider.GetPolicyAsync(new DataSetStorageAllocation(dataSet,
            new DataSetScope(tenant, organization, project)));

        Assert.Equal(900, result.TenantLimitBytes);
        Assert.Equal(700, result.OrganizationLimitBytes);
        Assert.Equal(500, result.ProjectLimitBytes);
        Assert.Equal(300, result.DataSetLimitBytes);
        Assert.Equal(65, result.WarningThresholdPercent);
    }

    [Fact]
    public async Task ChildPolicyCannotRelaxInheritedLimit()
    {
        var tenant = Guid.NewGuid();
        var project = Guid.NewGuid();
        var dataSet = new DataSetId(Guid.NewGuid());
        var provider = new HierarchicalDataSetStorageQuotaPolicyProvider(
            new DataSetStorageQuotaPolicy(500, 400, 300, 200),
            [
                Assignment(DataSetStorageQuotaScope.Tenant, tenant, new(900, null, null, null)),
                Assignment(DataSetStorageQuotaScope.Project, project, new(null, null, 900, null)),
                Assignment(DataSetStorageQuotaScope.DataSet, dataSet.Value, new(null, null, null, 900))
            ]);

        var result = await provider.GetPolicyAsync(new DataSetStorageAllocation(dataSet,
            new DataSetScope(tenant, Guid.NewGuid(), project)));

        Assert.Equal(500, result.TenantLimitBytes);
        Assert.Equal(300, result.ProjectLimitBytes);
        Assert.Equal(200, result.DataSetLimitBytes);
    }

    [Fact]
    public async Task SiblingTenantPoliciesDoNotLeakAcrossScopes()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var provider = new HierarchicalDataSetStorageQuotaPolicyProvider(
            new DataSetStorageQuotaPolicy(null, null, null, null),
            [Assignment(DataSetStorageQuotaScope.Tenant, tenantA, new(100, null, null, null))]);

        var policyA = await provider.GetPolicyAsync(Allocation(tenantA));
        var policyB = await provider.GetPolicyAsync(Allocation(tenantB));

        Assert.Equal(100, policyA.TenantLimitBytes);
        Assert.Null(policyB.TenantLimitBytes);
    }

    [Fact]
    public void RejectsDuplicateAssignmentsForSameTarget()
    {
        var tenant = Guid.NewGuid();
        var assignment = Assignment(DataSetStorageQuotaScope.Tenant, tenant, new(100, null, null, null));
        Assert.Throws<ArgumentException>(() => new HierarchicalDataSetStorageQuotaPolicyProvider(
            new DataSetStorageQuotaPolicy(null, null, null, null), [assignment, assignment]));
    }

    private static DataSetStorageQuotaPolicyAssignment Assignment(
        DataSetStorageQuotaScope scope, Guid id, DataSetStorageQuotaPolicy policy) => new(scope, id, policy);

    private static DataSetStorageAllocation Allocation(Guid tenant) =>
        new(new DataSetId(Guid.NewGuid()), new DataSetScope(tenant, Guid.NewGuid(), Guid.NewGuid()));
}
