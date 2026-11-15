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

    public async Task<DataSetPage> GetPageAsync(Guid tenantId, Guid? organizationId, Guid? projectId,
        string? search, DataSetStatus? status, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        var query = dbContext.DataSets.AsNoTracking().Include(x => x.SchemaRevisions)
            .Where(x => x.TenantId == tenantId);
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);
        if (projectId.HasValue) query = query.Where(x => x.ProjectId == projectId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search.Trim()));
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = (await query.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id.Value)
            .Skip(checked((page - 1) * pageSize))
            .Take(pageSize)
            .ToArray();
        return new DataSetPage(items, totalCount, page, pageSize);
    }

    public void Add(DataSetAggregate dataSet) => dbContext.DataSets.Add(dataSet);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Owned collection의 복합 키(DataSetId + Version)가 이미 설정되어 있으면
        // EF가 새 revision을 Modified로 추론하므로, 불변 revision의 새 항목만 Added로 확정합니다.
        foreach (var revision in dbContext.ChangeTracker.Entries<DataSetSchemaRevision>()
                     .Where(entry => entry.State == EntityState.Modified))
            revision.State = EntityState.Added;

        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
