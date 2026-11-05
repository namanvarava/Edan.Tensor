using EDAN.Tensor.DataSet.Application.Abstractions;

namespace EDAN.Tensor.DataSet.Application.UseCases;

/// <summary>
/// 기본 정책에 Tenant → Organization → Project → DataSet 순으로 정책을 결합합니다.
/// 하위 범위는 상위 한도를 완화할 수 없으며, 동일한 범위의 형제 객체는 서로 영향을 주지 않습니다.
/// </summary>
public sealed class HierarchicalDataSetStorageQuotaPolicyProvider : IDataSetStorageQuotaPolicyProvider
{
    private readonly DataSetStorageQuotaPolicy defaultPolicy;
    private readonly IReadOnlyList<DataSetStorageQuotaPolicyAssignment> assignments;

    public HierarchicalDataSetStorageQuotaPolicyProvider(
        DataSetStorageQuotaPolicy defaultPolicy,
        IEnumerable<DataSetStorageQuotaPolicyAssignment>? assignments = null)
    {
        this.defaultPolicy = Validate(defaultPolicy);
        var configured = (assignments ?? []).ToArray();
        foreach (var assignment in configured)
        {
            ArgumentNullException.ThrowIfNull(assignment);
            assignment.Validate();
        }

        if (configured.GroupBy(x => (x.Scope, x.TargetId)).Any(x => x.Count() > 1))
            throw new ArgumentException("동일한 범위와 대상에는 하나의 Quota 정책만 지정할 수 있습니다.", nameof(assignments));

        this.assignments = configured;
    }

    public Task<DataSetStorageQuotaPolicy> GetPolicyAsync(
        DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        cancellationToken.ThrowIfCancellationRequested();

        var scope = allocation.Scope;
        var resolved = defaultPolicy;
        resolved = Apply(resolved, DataSetStorageQuotaScope.Tenant, scope.TenantId);
        resolved = Apply(resolved, DataSetStorageQuotaScope.Organization, scope.OrganizationId);
        resolved = Apply(resolved, DataSetStorageQuotaScope.Project, scope.ProjectId);
        resolved = Apply(resolved, DataSetStorageQuotaScope.DataSet, allocation.DataSetId.Value);
        return Task.FromResult(resolved);
    }

    private DataSetStorageQuotaPolicy Apply(DataSetStorageQuotaPolicy current, DataSetStorageQuotaScope scope, Guid id)
    {
        var assignment = assignments.SingleOrDefault(x => x.Scope == scope && x.TargetId == id);
        if (assignment is null) return current;
        var next = assignment.Policy;
        return new DataSetStorageQuotaPolicy(
            Tighter(current.TenantLimitBytes, next.TenantLimitBytes),
            Tighter(current.OrganizationLimitBytes, next.OrganizationLimitBytes),
            Tighter(current.ProjectLimitBytes, next.ProjectLimitBytes),
            Tighter(current.DataSetLimitBytes, next.DataSetLimitBytes),
            next.WarningThresholdPercent);
    }

    private static long? Tighter(long? inherited, long? local) =>
        inherited is null ? local : local is null ? inherited : Math.Min(inherited.Value, local.Value);

    private static DataSetStorageQuotaPolicy Validate(DataSetStorageQuotaPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (policy.TenantLimitBytes is < 0 || policy.OrganizationLimitBytes is < 0
            || policy.ProjectLimitBytes is < 0 || policy.DataSetLimitBytes is < 0)
            throw new ArgumentOutOfRangeException(nameof(policy), "Quota limits cannot be negative.");
        if (policy.WarningThresholdPercent is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(policy), "Warning threshold must be between 1 and 100.");
        return policy;
    }
}
