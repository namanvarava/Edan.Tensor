using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskRepositoryTests
{
    [Fact]
    public async Task ReadyTenantQueryReturnsOnlyTenantsWithDueWork()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var now = DateTimeOffset.UtcNow;
        var readyTenant = Guid.NewGuid();
        var secondReadyTenant = Guid.NewGuid();
        var delayedTenant = Guid.NewGuid();

        db.WorkerTasks.Add(WorkerTask.Create(readyTenant, "test.ready", "ready-1", "payload://1", now));
        db.WorkerTasks.Add(WorkerTask.Create(secondReadyTenant, "test.ready", "ready-2", "payload://2", now.AddMinutes(-1)));
        var delayed = WorkerTask.Create(delayedTenant, "test.delayed", "delayed-1", "payload://3", now);
        delayed.AcquireLease("test-worker", Guid.NewGuid(), now, now.AddMinutes(1));
        delayed.Start(delayed.LeaseId!.Value, delayed.FencingToken, now);
        delayed.Fail(delayed.LeaseId!.Value, delayed.FencingToken, "test.retry", now, now.AddHours(1));
        db.WorkerTasks.Add(delayed);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var tenants = await repository.GetReadyTenantIdsAsync(now, 10);

        Assert.Equal(2, tenants.Count);
        Assert.Contains(readyTenant, tenants);
        Assert.Contains(secondReadyTenant, tenants);
        Assert.DoesNotContain(delayedTenant, tenants);
    }
}
