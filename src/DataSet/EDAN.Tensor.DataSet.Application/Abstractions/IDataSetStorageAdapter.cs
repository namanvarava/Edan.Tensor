using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.DataSet.Application.Abstractions;

/// <summary>물리 저장소를 추상화하며 경로와 연결 문자열을 Application에 노출하지 않습니다.</summary>
public interface IDataSetStorageAdapter
{
    string AdapterId { get; }
    Task<StorageReference> AllocateAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default);
    Task WriteAsync(DataSetStorageAllocation allocation, StorageReference reference, Stream content, DataSetStorageQuotaPolicy quotaPolicy, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(DataSetStorageAllocation allocation, StorageReference reference, CancellationToken cancellationToken = default);
    Task ReleaseAsync(DataSetStorageAllocation allocation, StorageReference reference, CancellationToken cancellationToken = default);
}

/// <summary>저장소 작업은 DataSet ID와 소유 범위를 함께 전달하여 교차 Tenant 접근을 방지합니다.</summary>
public sealed record DataSetStorageAllocation(DataSetId DataSetId, DataSetScope Scope);

/// <summary>설정된 저장소 어댑터를 조회하는 계약입니다.</summary>
public interface IDataSetStorageAdapterResolver
{
    IDataSetStorageAdapter? Resolve(string adapterId);
}

/// <summary>소유 범위별 콘텐츠 사용량을 반환합니다. 값은 실제 저장된 바이트 기준입니다.</summary>
public interface IDataSetStorageUsageReader
{
    Task<DataSetStorageUsage> GetUsageAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default);
}

/// <summary>Tenant 전체의 실제 콘텐츠 저장량입니다. 메타데이터가 아닌 콘텐츠 DB 기준입니다.</summary>
public sealed record DataSetTenantStorageUsage(long TenantBytes, long TenantDataSetCount);

/// <summary>Tenant 전체 저장량 조회 계약입니다. Tenant ID 이외의 소유 범위는 입력받지 않습니다.</summary>
public interface IDataSetTenantStorageUsageReader
{
    Task<DataSetTenantStorageUsage> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>현재 DataSet 및 그 소유 Project 범위의 콘텐츠 사용량 스냅샷입니다.</summary>
public sealed record DataSetStorageUsage(
    long TenantBytes, long TenantDataSetCount,
    long OrganizationBytes, long OrganizationDataSetCount,
    long ProjectBytes, long ProjectDataSetCount,
    long DataSetBytes);

/// <summary>Tenant·Organization·Project·DataSet 계층의 저장 용량 한도입니다. null은 무제한입니다.</summary>
public sealed record DataSetStorageQuotaPolicy(long? TenantLimitBytes, long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes, int WarningThresholdPercent = 80);

/// <summary>소유 범위별 저장 용량 정책을 제공합니다.</summary>
public interface IDataSetStorageQuotaPolicyProvider
{
    Task<DataSetStorageQuotaPolicy> GetPolicyAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default);
}

/// <summary>저장 용량 한도 초과로 콘텐츠 쓰기가 거부되었습니다.</summary>
public sealed class DataSetStorageQuotaExceededException(string scope, long limitBytes, long projectedBytes)
    : Exception($"DataSet storage quota exceeded for {scope}: limit {limitBytes} bytes, projected {projectedBytes} bytes.")
{
    public string Scope { get; } = scope;
    public long LimitBytes { get; } = limitBytes;
    public long ProjectedBytes { get; } = projectedBytes;
}

/// <summary>계층형 DataSet 저장 용량 정책의 적용 범위입니다.</summary>
public enum DataSetStorageQuotaScope { Tenant, Organization, Project, DataSet }

/// <summary>특정 Tenant·Organization·Project·DataSet에 적용할 정책 할당입니다.</summary>
public sealed record DataSetStorageQuotaPolicyAssignment(
    DataSetStorageQuotaScope Scope, Guid TargetId, DataSetStorageQuotaPolicy Policy)
{
    public void Validate()
    {
        if (TargetId == Guid.Empty) throw new ArgumentException("Quota 정책 대상 ID는 비어 있을 수 없습니다.", nameof(TargetId));
        ArgumentNullException.ThrowIfNull(Policy);
        if (Policy.TenantLimitBytes is < 0 || Policy.OrganizationLimitBytes is < 0
            || Policy.ProjectLimitBytes is < 0 || Policy.DataSetLimitBytes is < 0)
            throw new ArgumentOutOfRangeException(nameof(Policy), "Quota limits cannot be negative.");
        if (Policy.WarningThresholdPercent is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(Policy), "Warning threshold must be between 1 and 100.");
    }
}
