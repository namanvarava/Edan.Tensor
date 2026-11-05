using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.DataSet.Application.Abstractions;

/// <summary>물리 저장소를 추상화하며 경로와 연결 문자열을 Application에 노출하지 않습니다.</summary>
public interface IDataSetStorageAdapter
{
    string AdapterId { get; }
    Task<StorageReference> AllocateAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default);
    Task ReleaseAsync(StorageReference reference, CancellationToken cancellationToken = default);
}

/// <summary>저장소 할당에 필요한 논리 식별자만 전달합니다.</summary>
public sealed record DataSetStorageAllocation(DataSetId DataSetId, DataSetScope Scope);

/// <summary>설정된 저장소 어댑터를 조회하는 계약입니다.</summary>
public interface IDataSetStorageAdapterResolver
{
    IDataSetStorageAdapter? Resolve(string adapterId);
}
