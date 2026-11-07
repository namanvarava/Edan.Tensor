using EDAN.Tensor.DataSet.Domain.Model;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.DataSet.Infrastructure.Validation;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
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
            var details = differences.Select(operation => operation is AlterColumnOperation alter
                ? $"{alter.Table}.{alter.Name}: old(type={alter.OldColumn.ColumnType ?? "<null>"}, nullable={alter.OldColumn.IsNullable}, max={alter.OldColumn.MaxLength?.ToString() ?? "<null>"}); new(type={alter.ColumnType ?? "<null>"}, nullable={alter.IsNullable}, max={alter.MaxLength?.ToString() ?? "<null>"})"
                : operation.GetType().Name);
            Assert.True(differences.Count == 0, "DataSet Migration Snapshot 차이: " + string.Join("; ", details));
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
        Assert.Contains(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.GetColumnName()).SequenceEqual(
                new[] { "TenantId", "OrganizationId", "ProjectId" }));
    }

    [Fact]
    public async Task ValidationRepository_IsTenantScopedAndDeletesOnlyExplicitly()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DataSetDbContext>().UseSqlite(connection).Options;
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var profileJson = "{\"fields\":[{\"name\":\"id\",\"type\":\"integer\"}]}";

        await using var db = new DataSetDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new EfDataSetValidationRepository(db);
        var profile = await repository.CreateProfileAsync(tenantA, "기본", profileJson, DateTimeOffset.UtcNow);

        Assert.NotNull(await repository.GetProfileAsync(tenantA, profile.Id, 1));
        Assert.Null(await repository.GetProfileAsync(tenantB, profile.Id, 1));

        var execution = await repository.CreateExecutionAsync(tenantA, Guid.NewGuid(), 1, 1,
            profile.Id, 1, DateTimeOffset.UtcNow);
        Assert.True(await repository.TryStartAsync(tenantA, execution.Id, 10));
        Assert.True(await repository.RecordRowAsync(tenantA, execution.Id, 10, 1,
            [new DataSetValidationFinding("field.id.type", DataSetValidationSeverity.Error, "id", "형식이 올바르지 않습니다.")],
            [new DataSetValidationFinding("field.id.type", DataSetValidationSeverity.Error, "id", "형식이 올바르지 않습니다.")],
            [], 1, 1, false));
        Assert.True(await repository.CompleteAsync(tenantA, execution.Id, 10,
            DataSetValidationStatus.Invalid, 1, false, "{\"field.id.type\":1}"));

        Assert.Null(await repository.GetExecutionAsync(tenantB, execution.Id));
        Assert.Single(await repository.GetDetailsAsync(tenantA, execution.Id, 0, 10));
        Assert.True(await repository.DeleteExecutionAsync(tenantA, execution.Id));
        Assert.Null(await repository.GetExecutionAsync(tenantA, execution.Id));
    }

    [Fact]
    public async Task StaleFencingTokenCannotWriteValidationResultAfterRetryTakesOwnership()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DataSetDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();

        await using var db = new DataSetDbContext(options);
        await db.Database.MigrateAsync();
        var repository = new EfDataSetValidationRepository(db);
        var execution = await repository.CreateExecutionAsync(tenantId, Guid.NewGuid(), 1, 1,
            Guid.NewGuid(), 1, DateTimeOffset.UtcNow);

        Assert.True(await repository.TryStartAsync(tenantId, execution.Id, 10));
        Assert.True(await repository.MarkQueuedAsync(tenantId, execution.Id, 10));
        Assert.True(await repository.TryStartAsync(tenantId, execution.Id, 11));

        Assert.False(await repository.RecordRowAsync(tenantId, execution.Id, 10, 1,
            [new DataSetValidationFinding("field.id.type", DataSetValidationSeverity.Error, "id", "오래된 실행")],
            [], [], 1, 1, false));
        Assert.False(await repository.CompleteAsync(tenantId, execution.Id, 10,
            DataSetValidationStatus.Invalid, 1, false, "{}"));

        var current = await repository.GetExecutionAsync(tenantId, execution.Id);
        Assert.NotNull(current);
        Assert.Equal(11, current.FencingToken);
        Assert.Equal(DataSetValidationStatus.Running, current.Status);
        Assert.Equal(0, current.ProcessedRecords);
        Assert.Empty(await repository.GetDetailsAsync(tenantId, execution.Id, 0, 10));
    }
}
