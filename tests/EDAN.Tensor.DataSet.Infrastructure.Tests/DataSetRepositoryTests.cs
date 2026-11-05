using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DataSetAggregate = EDAN.Tensor.DataSet.Domain.Model.DataSet;

namespace EDAN.Tensor.DataSet.Infrastructure.Tests;

public sealed class DataSetRepositoryTests
{
    [Fact]
    public async Task SaveAndLoad_PersistsAggregateAndSchemaRevisions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DataSetDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var id = new DataSetId(Guid.NewGuid());

        await using (var db = new DataSetDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            var aggregate = DataSetAggregate.Create(id,
                new DataSetScope(tenantId, Guid.NewGuid(), Guid.NewGuid()),
                "Sales", new StorageReference("storage:dataset-01"), "sqlite");
            aggregate.AddSchema("{\"type\":\"object\"}", DateTimeOffset.UtcNow);
            db.DataSets.Add(aggregate);
            await db.SaveChangesAsync();
        }

        await using (var db = new DataSetDbContext(options))
        {
            var repository = new EfDataSetRepository(db);
            var loaded = await repository.GetAsync(id, tenantId);
            Assert.NotNull(loaded);
            Assert.Equal("Sales", loaded.Name);
            Assert.Equal(1, loaded.SchemaRevisions.Count);
            Assert.Equal("{\"type\":\"object\"}", loaded.SchemaRevisions[0].SchemaJson);
            Assert.Null(await repository.GetAsync(id, Guid.NewGuid()));
        }
    }

    [Fact]
    public async Task Version_IsConfiguredAsConcurrencyToken()
    {
        var options = new DbContextOptionsBuilder<DataSetDbContext>()
            .UseSqlite("Data Source=:memory:").Options;
        await using var db = new DataSetDbContext(options);
        var entity = db.Model.FindEntityType(typeof(DataSetAggregate));
        Assert.NotNull(entity);
        Assert.True(entity.FindProperty(nameof(DataSetAggregate.Version))!.IsConcurrencyToken);
    }

    [Fact]
    public async Task TenantScope_IsIndexedForScopedQueries()
    {
        var options = new DbContextOptionsBuilder<DataSetDbContext>()
            .UseSqlite("Data Source=:memory:").Options;
        await using var db = new DataSetDbContext(options);
        var entity = db.Model.FindEntityType(typeof(DataSetAggregate))!;
        Assert.Contains(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(
                new[] { "Scope_TenantId", "Scope_OrganizationId", "Scope_ProjectId" }));
    }
}
