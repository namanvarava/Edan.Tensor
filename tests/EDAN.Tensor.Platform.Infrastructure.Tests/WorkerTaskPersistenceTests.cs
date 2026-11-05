using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Platform.Infrastructure.Tests;

public sealed class WorkerTaskPersistenceTests
{
    [Fact]
    public async Task SqlitePersistsWorkerTaskAndEnforcesTenantScopedIdempotency()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<PlatformDbContext>().UseSqlite(connection).Options;
        await using var db = new PlatformDbContext(options);
        await db.Database.MigrateAsync();

        var tenantA = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("WORKER-A"), "작업 Tenant A", DateTimeOffset.UtcNow);
        var tenantB = Tenant.Create(new TenantId(Guid.NewGuid()), new TenantCode("WORKER-B"), "작업 Tenant B", DateTimeOffset.UtcNow);
        db.Tenants.AddRange(tenantA, tenantB);
        await db.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantA.Id, "archive.generate", "same-key", "payload://archive/a", now);
        var repository = new WorkerTaskRepository(db);
        repository.Add(task);
        await db.SaveChangesAsync();

        Assert.NotNull(await repository.GetAsync(tenantA.Id, task.Id));
        Assert.Null(await repository.GetAsync(tenantB.Id, task.Id));
        Assert.Single(await repository.GetReadyAsync(tenantA.Id, now, 10));

        repository.Add(WorkerTask.Create(tenantA.Id, "archive.generate", "same-key", "payload://archive/duplicate", now));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());

        repository.Add(WorkerTask.Create(tenantB.Id, "archive.generate", "same-key", "payload://archive/b", now));
        await db.SaveChangesAsync();
        Assert.Single(await repository.GetReadyAsync(tenantB.Id, now, 10));
    }

    [Fact]
    public void WorkerTaskVersionIsAnOptimisticConcurrencyToken()
    {
        using var db = CreateContext();
        var entity = db.Model.FindEntityType(typeof(WorkerTask))!;
        var version = entity.FindProperty(nameof(WorkerTask.Version))!;
        Assert.True(version.IsConcurrencyToken);
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(WorkerTask.TenantId), nameof(WorkerTask.IdempotencyKey)]));
    }

    private static PlatformDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new PlatformDbContext(options);
    }
}
