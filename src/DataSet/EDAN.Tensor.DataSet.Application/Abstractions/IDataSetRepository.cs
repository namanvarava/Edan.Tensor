using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.DataSet.Application.Abstractions;

/// <summary>DataSet 메타데이터 저장소는 항상 Tenant 경계를 포함해 조회합니다.</summary>
public interface IDataSetRepository
{
    Task<DataSet?> GetAsync(DataSetId id, Guid tenantId, CancellationToken cancellationToken = default);
    void Add(DataSet dataSet);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>플랫폼 소유 관계를 확인하는 외부 경계 포트입니다.</summary>
public interface IDataSetScopeValidator
{
    Task<bool> IsValidAsync(DataSetScope scope, CancellationToken cancellationToken = default);
}
