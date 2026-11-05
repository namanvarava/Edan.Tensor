using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
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
            var migrationsAssembly = db.GetService<IMigrationsAssembly>();
            var differ = db.GetService<IMigrationsModelDiffer>();
            var currentModel = db.GetService<IDesignTimeModel>().Model;
            var snapshotModel = migrationsAssembly.ModelSnapshot!.Model;
            var initializedSnapshot = db.GetService<IModelRuntimeInitializer>().Initialize(snapshotModel);
            var differences = differ.GetDifferences(initializedSnapshot.GetRelationalModel(), currentModel.GetRelationalModel());
            static string DescribeModel(IModel model) => string.Join(" | ", model.GetEntityTypes()
                .Where(entity => entity.GetTableName() == "datasets")
                .Select(entity => $"Entity={entity.DisplayName()}; Properties={string.Join(",", entity.GetProperties().Select(property => property.Name + ":" + property.GetColumnName()))}; FKs={string.Join(",", entity.GetForeignKeys().Select(fk => fk.Properties.First().Name + "->" + fk.PrincipalEntityType.DisplayName()))}"));
            Assert.True(differences.Count == 0,
                "DataSet Migration Snapshot 차이: " + string.Join("; ", differences.Select(x => x switch
                {
                    Microsoft.EntityFrameworkCore.Migrations.Operations.DropForeignKeyOperation fk => $"DropForeignKey table={fk.Table}, name={fk.Name}, schema={fk.Schema}",
                    Microsoft.EntityFrameworkCore.Migrations.Operations.DropColumnOperation column => $"DropColumn table={column.Table}, name={column.Name}, schema={column.Schema}",
                    _ => x.GetType().Name
                })) + $" | Snapshot: {DescribeModel(initializedSnapshot)} | Runtime: {DescribeModel(currentModel)}");
            await db.Database.MigrateAsync();
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
            Assert.Single(loaded.SchemaRevisions);
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
        var scope = entity.FindNavigation(nameof(DataSetAggregate.Scope))!.TargetEntityType;
        Assert.Contains(scope.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual(
                new[] { nameof(DataSetScope.TenantId), nameof(DataSetScope.OrganizationId), nameof(DataSetScope.ProjectId) }));
    }
}
