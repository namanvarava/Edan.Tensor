using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;
using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.DataSet.Application.Abstractions;

/// <summary>DataSet 메타데이터 저장소는 항상 Tenant 경계를 포함해 조회합니다.</summary>
public interface IDataSetRepository
{
    Task<DataSetAggregate?> GetAsync(DataSetId id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<DataSetPage> GetPageAsync(Guid tenantId, Guid? organizationId, Guid? projectId,
        string? search, DataSetStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("DataSet 페이지 조회가 구현되지 않았습니다.");
    void Add(DataSetAggregate dataSet);
    void Remove(DataSetAggregate dataSet);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>Tenant 범위 DataSet의 검색·필터·페이징 결과입니다.</summary>
public sealed record DataSetPage(
    IReadOnlyList<DataSetAggregate> Items, int TotalCount, int Page, int PageSize);

/// <summary>플랫폼 소유 관계를 확인하는 외부 경계 포트입니다.</summary>
public interface IDataSetScopeValidator
{
    Task<bool> IsValidAsync(DataSetScope scope, CancellationToken cancellationToken = default);
}
