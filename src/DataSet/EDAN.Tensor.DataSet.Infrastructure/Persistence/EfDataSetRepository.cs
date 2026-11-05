using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Domain.Model;
using Microsoft.EntityFrameworkCore;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;

namespace EDAN.Tensor.DataSet.Infrastructure.Persistence;

/// <summary>Tenant 조건을 조회 키에 항상 포함하는 EF Core 저장소 구현입니다.</summary>
public sealed class EfDataSetRepository(DataSetDbContext dbContext) : IDataSetRepository
{
    public Task<DataSetAggregate?> GetAsync(DataSetId id, Guid tenantId, CancellationToken cancellationToken = default) =>
        dbContext.DataSets
            .Include(x => x.SchemaRevisions)
            .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

    public void Add(DataSetAggregate dataSet) => dbContext.DataSets.Add(dataSet);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
