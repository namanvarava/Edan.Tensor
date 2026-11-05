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
        await repository.SaveChangesAsync();

        Assert.NotNull(await repository.GetAsync(tenantA, task.Id));
        Assert.Null(await repository.GetAsync(tenantB, task.Id));
        Assert.Single(await repository.GetReadyAsync(tenantA, now, 10));

        repository.Add(WorkerTask.Create(tenantA, "archive.generate", "same-key", "payload://duplicate", now));
        await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
        db.ChangeTracker.Clear();

        repository.Add(WorkerTask.Create(tenantB, "archive.generate", "same-key", "payload://archive/b", now));
        await repository.SaveChangesAsync();
        Assert.Single(await repository.GetReadyAsync(tenantB, now, 10));
    }

    [Fact]
    public async Task LeaseCanBeAcquiredOnlyOnceAndRemainsTenantScoped()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantA, "archive.generate", "lease-key", "payload://archive/a", now);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var leaseId = Guid.NewGuid();
        var acquired = await repository.TryAcquireLeaseAsync(
            tenantA, task.Id, "worker-a", leaseId, now, now.AddMinutes(5));

        Assert.NotNull(acquired);
        Assert.Equal(1, acquired.FencingToken);
        Assert.Equal(leaseId, acquired.LeaseId);
        Assert.Equal(WorkerTaskStatus.Leased, acquired.Status);

        var duplicate = await repository.TryAcquireLeaseAsync(
            tenantA, task.Id, "worker-b", Guid.NewGuid(), now, now.AddMinutes(5));
        var crossTenant = await repository.TryAcquireLeaseAsync(
            tenantB, task.Id, "worker-b", Guid.NewGuid(), now, now.AddMinutes(5));

        Assert.Null(duplicate);
        Assert.Null(crossTenant);
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
