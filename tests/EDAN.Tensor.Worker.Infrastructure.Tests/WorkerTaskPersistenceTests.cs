using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskPersistenceTests
{
    [Fact]
    public async Task SqlitePersistsTasksAndEnforcesTenantScopedIdempotency()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var repository = new WorkerTaskRepository(db);
        var task = WorkerTask.Create(tenantA, "archive.generate", "same-key", "payload://archive/a", now);
        repository.Add(task);
        await db.SaveChangesAsync();

        Assert.NotNull(await repository.GetAsync(tenantA, task.Id));
        Assert.Null(await repository.GetAsync(tenantB, task.Id));
        Assert.Single(await repository.GetReadyAsync(tenantA, now, 10));

        repository.Add(WorkerTask.Create(tenantA, "archive.generate", "same-key", "payload://duplicate", now));
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();

        repository.Add(WorkerTask.Create(tenantB, "archive.generate", "same-key", "payload://archive/b", now));
        await db.SaveChangesAsync();
        Assert.Single(await repository.GetReadyAsync(tenantB, now, 10));
    }

    [Fact]
    public void VersionIsOptimisticConcurrencyToken()
    {
        var options = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options;
        using var db = new WorkerDbContext(options);
        var entity = db.Model.FindEntityType(typeof(WorkerTask))!;
        Assert.True(entity.FindProperty(nameof(WorkerTask.Version))!.IsConcurrencyToken);
    }
}
