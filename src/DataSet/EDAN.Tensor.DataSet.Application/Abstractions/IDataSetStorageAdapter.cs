using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.DataSet.Application.Abstractions;

/// <summary>물리 저장소를 추상화하며 경로와 연결 문자열을 Application에 노출하지 않습니다.</summary>
public interface IDataSetStorageAdapter
{
    string AdapterId { get; }
    Task<StorageReference> AllocateAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default);
    Task WriteAsync(DataSetStorageAllocation allocation, StorageReference reference, Stream content, CancellationToken cancellationToken = default);
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

/// <summary>현재 DataSet 및 그 소유 Project 범위의 콘텐츠 사용량 스냅샷입니다.</summary>
public sealed record DataSetStorageUsage(long ScopeBytes, long ScopeDataSetCount, long DataSetBytes);
