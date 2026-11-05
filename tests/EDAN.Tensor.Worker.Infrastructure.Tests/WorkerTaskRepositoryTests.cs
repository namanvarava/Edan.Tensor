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

        var ordered = await repository.GetReadyAsync(readyTenant, now, 10);
        Assert.Single(ordered);
        Assert.Equal("ready-1", ordered[0].IdempotencyKey);
    }

    [Fact]
    public async Task ReadyQueriesUsePersistedUtcMillisecondsAndExcludeFutureRetries()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var now = DateTimeOffset.UtcNow;
        var tenantId = Guid.NewGuid();
        var early = WorkerTask.Create(tenantId, "test.ready", "early", "payload://early", now.AddMinutes(-2));
        var later = WorkerTask.Create(tenantId, "test.ready", "later", "payload://later", now.AddMinutes(-1));
        var future = WorkerTask.Create(tenantId, "test.retry", "future", "payload://future", now);
        future.AcquireLease("worker", Guid.NewGuid(), now, now.AddMinutes(1));
        future.Start(future.LeaseId!.Value, future.FencingToken, now);
        future.Fail(future.LeaseId!.Value, future.FencingToken, "test.retry", now, now.AddHours(1));
        db.WorkerTasks.AddRange(later, future, early);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var ready = await repository.GetReadyAsync(tenantId, now, 10);
        Assert.Equal(new[] { "early", "later" }, ready.Select(x => x.IdempotencyKey));
        Assert.DoesNotContain(ready, x => x.IdempotencyKey == "future");
    }

    [Fact]
    public async Task DeadLetterQueryPagesInDatabaseOrderWithoutCrossTenantResults()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var oldest = WorkerTask.Create(tenantId, "test.dlq", "oldest", "payload://oldest", now.AddMinutes(-3), maxAttempts: 1);
        var middle = WorkerTask.Create(tenantId, "test.dlq", "middle", "payload://middle", now.AddMinutes(-2), maxAttempts: 1);
        var newest = WorkerTask.Create(tenantId, "test.dlq", "newest", "payload://newest", now.AddMinutes(-1), maxAttempts: 1);
        var other = WorkerTask.Create(otherTenantId, "test.dlq", "other", "payload://other", now, maxAttempts: 1);
        foreach (var task in new[] { oldest, middle, newest, other })
        {
            var lease = Guid.NewGuid();
            task.AcquireLease("worker", lease, now, now.AddMinutes(1));
            task.Start(lease, task.FencingToken, now);
            task.Fail(lease, task.FencingToken, "test.failed", now, now.AddMinutes(2));
        }
        db.WorkerTasks.AddRange(oldest, middle, newest, other);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var firstPage = await repository.GetDeadLetteredAsync(tenantId, 0, 2);
        var secondPage = await repository.GetDeadLetteredAsync(tenantId, 2, 2);

        Assert.Equal(new[] { "newest", "middle" }, firstPage.Select(x => x.IdempotencyKey));
        Assert.Equal(new[] { "oldest" }, secondPage.Select(x => x.IdempotencyKey));
    }

    [Fact]
    public async Task ExpiredRunningLeaseIsRecoveredAndCanBeReacquiredWithNewFencingToken()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantId, "test.recovery", "recovery-key", "payload://test", now, maxAttempts: 3);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();
        var repository = new WorkerTaskRepository(db);
        var oldLease = Guid.NewGuid();
        var leased = await repository.TryAcquireLeaseAsync(tenantId, task.Id, "worker-old", oldLease, now, now.AddMinutes(1));
        Assert.NotNull(leased);
        Assert.True(await repository.TryStartAsync(tenantId, task.Id, oldLease, leased.FencingToken, now));
        Assert.Equal(1, await repository.RecoverExpiredLeasesAsync(now.AddMinutes(2), 100));
        var recovered = await repository.GetAsync(tenantId, task.Id);
        Assert.Equal(WorkerTaskStatus.RetryScheduled, recovered!.Status);
        Assert.Equal("worker.lease.expired", recovered.LastErrorCode);
        Assert.Null(recovered.LeaseId);
        var newLease = Guid.NewGuid();
        var reacquired = await repository.TryAcquireLeaseAsync(tenantId, task.Id, "worker-new", newLease, now.AddMinutes(2), now.AddMinutes(7));
        Assert.NotNull(reacquired);
        Assert.Equal(2, reacquired.FencingToken);
        Assert.False(await repository.TryCompleteAsync(tenantId, task.Id, oldLease, leased.FencingToken, now.AddMinutes(3)));
    }

    [Fact]
    public async Task ExpiredLeaseAtMaximumAttemptsMovesDirectlyToDeadLetter()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantId, "test.recovery", "recovery-dlq", "payload://test", now, maxAttempts: 1);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();
        var repository = new WorkerTaskRepository(db);
        var leaseId = Guid.NewGuid();
        var leased = await repository.TryAcquireLeaseAsync(tenantId, task.Id, "worker-crashed", leaseId, now, now.AddMinutes(1));
        Assert.NotNull(leased);
        Assert.True(await repository.TryStartAsync(tenantId, task.Id, leaseId, leased.FencingToken, now));
        Assert.Equal(1, await repository.RecoverExpiredLeasesAsync(now.AddMinutes(2), 100));
        var recovered = await repository.GetAsync(tenantId, task.Id);
        Assert.Equal(WorkerTaskStatus.DeadLettered, recovered!.Status);
        Assert.Equal("worker.lease.expired", recovered.LastErrorCode);
        Assert.Null(recovered.NextAttemptAtUtc);
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryProcessesOnlyTheOldestRequestedBatch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var tasks = new[]
        {
            WorkerTask.Create(tenantId, "test.recovery", "expired-1", "payload://1", now),
            WorkerTask.Create(tenantId, "test.recovery", "expired-2", "payload://2", now),
            WorkerTask.Create(tenantId, "test.recovery", "expired-3", "payload://3", now)
        };
        for (var index = 0; index < tasks.Length; index++)
        {
            var leaseId = Guid.NewGuid();
            var expiry = now.AddMinutes(index + 1);
            tasks[index].AcquireLease("worker", leaseId, now, expiry);
            tasks[index].Start(leaseId, tasks[index].FencingToken, now);
        }
        db.WorkerTasks.AddRange(tasks);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var recovered = await repository.RecoverExpiredLeasesAsync(now.AddMinutes(5), 2);

        Assert.Equal(2, recovered);
        Assert.Equal(WorkerTaskStatus.RetryScheduled, (await repository.GetAsync(tenantId, tasks[0].Id))!.Status);
        Assert.Equal(WorkerTaskStatus.RetryScheduled, (await repository.GetAsync(tenantId, tasks[1].Id))!.Status);
        Assert.Equal(WorkerTaskStatus.Running, (await repository.GetAsync(tenantId, tasks[2].Id))!.Status);
    }

    [Fact]
    public async Task LeaseRenewalFromAnotherContextDoesNotPreventCompletionInOriginalContext()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantId, "test.renewal", "renewal-completion",
            "payload://renewal", now);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var leaseId = Guid.NewGuid();
        var leased = await repository.TryAcquireLeaseAsync(
            tenantId, task.Id, "worker-a", leaseId, now, now.AddMinutes(1));
        Assert.NotNull(leased);
        Assert.True(await repository.TryStartAsync(
            tenantId, task.Id, leaseId, leased.FencingToken, now));

        await using (var renewalDb = new WorkerDbContext(options))
        {
            var renewedTask = await renewalDb.WorkerTasks.SingleAsync(x => x.TenantId == tenantId && x.Id == task.Id);
            renewedTask.RenewLease(leaseId, renewedTask.FencingToken, now.AddSeconds(30), now.AddMinutes(10));
            await renewalDb.SaveChangesAsync();
        }

        Assert.True(await repository.TryCompleteAsync(
            tenantId, task.Id, leaseId, leased.FencingToken, now.AddMinutes(2), "result://complete"));

        var persisted = await db.WorkerTasks.AsNoTracking().SingleAsync(x => x.TenantId == tenantId && x.Id == task.Id);
        Assert.Equal(WorkerTaskStatus.Succeeded, persisted.Status);
        Assert.Equal("result://complete", persisted.ResultReference);
        Assert.Null(persisted.LeaseId);
        Assert.Null(persisted.LeaseExpiresAtUtc);
    }

    [Fact]
    public async Task ActiveCheckpointedLeaseIsNotReturnedAsReadyWork()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantId, "test.checkpointed", "checkpointed-active",
            "payload://checkpointed", now);
        var leaseId = Guid.NewGuid();
        var token = task.AcquireLease("worker-a", leaseId, now, now.AddMinutes(10));
        task.Start(leaseId, token, now);
        task.SaveCheckpoint(leaseId, token, "offset:42", now.AddMinutes(1));
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);

        Assert.Empty(await repository.GetReadyTenantIdsAsync(now.AddMinutes(2), 10));
        Assert.Empty(await repository.GetReadyAsync(tenantId, now.AddMinutes(2), 10));
    }

}
