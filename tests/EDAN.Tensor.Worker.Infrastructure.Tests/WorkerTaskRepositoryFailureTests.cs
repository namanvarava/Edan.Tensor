using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskRepositoryFailureTests
{
    [Fact]
    public async Task PersistenceFailureDetachesMutatedTaskAndDoesNotPersistPartialProgress()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid tenantId;
        Guid taskId;
        await using (var seedDb = new WorkerDbContext(baseOptions))
        {
            await seedDb.Database.MigrateAsync();
            tenantId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var task = WorkerTask.Create(tenantId, "test.persistence", "failure-key", "payload://failure", now);
            var leaseId = Guid.NewGuid();
            task.AcquireLease("worker", leaseId, now, now.AddMinutes(5));
            task.Start(leaseId, task.FencingToken, now);
            seedDb.WorkerTasks.Add(task);
            await seedDb.SaveChangesAsync();
            taskId = task.Id;
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new IOException("Simulated persistence failure.")))
            .Options;

        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);
            var tracked = await repository.GetAsync(tenantId, taskId);
            Assert.NotNull(tracked);

            await Assert.ThrowsAsync<IOException>(() => repository.TryReportProgressAsync(
                tenantId, taskId, tracked.LeaseId!.Value, tracked.FencingToken,
                50, "halfway", "must not persist", DateTimeOffset.UtcNow));

            Assert.Equal(EntityState.Detached, failingDb.Entry(tracked).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking()
            .SingleAsync(x => x.TenantId == tenantId && x.Id == taskId);
        Assert.Equal(0, persisted.ProgressPercent);
        Assert.Null(persisted.ProgressStage);
        Assert.Null(persisted.ProgressMessage);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Status);
    }

    [Fact]
    public async Task PersistenceCancellationDetachesMutatedTaskAndPropagatesCancellation()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid tenantId;
        Guid taskId;
        await using (var seedDb = new WorkerDbContext(baseOptions))
        {
            await seedDb.Database.MigrateAsync();
            tenantId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var task = WorkerTask.Create(tenantId, "test.persistence", "cancel-key", "payload://cancel", now);
            var leaseId = Guid.NewGuid();
            task.AcquireLease("worker", leaseId, now, now.AddMinutes(5));
            task.Start(leaseId, task.FencingToken, now);
            seedDb.WorkerTasks.Add(task);
            await seedDb.SaveChangesAsync();
            taskId = task.Id;
        }

        var cancelledOptions = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new OperationCanceledException("Simulated save cancellation.")))
            .Options;

        await using (var cancelledDb = new WorkerDbContext(cancelledOptions))
        {
            var repository = new WorkerTaskRepository(cancelledDb);
            var tracked = await repository.GetAsync(tenantId, taskId);
            Assert.NotNull(tracked);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.TryReportProgressAsync(
                tenantId, taskId, tracked.LeaseId!.Value, tracked.FencingToken,
                75, "cancelled-save", "must not persist", DateTimeOffset.UtcNow));

            Assert.Equal(EntityState.Detached, cancelledDb.Entry(tracked).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking()
            .SingleAsync(x => x.TenantId == tenantId && x.Id == taskId);
        Assert.Equal(0, persisted.ProgressPercent);
        Assert.Null(persisted.ProgressStage);
        Assert.Null(persisted.ProgressMessage);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Status);
    }


    [Fact]
    public async Task EnqueueCancellationDetachesAddedTaskAndPropagatesCancellation()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using (var setup = new WorkerDbContext(baseOptions)) await setup.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var task = WorkerTask.Create(tenantId, "test.enqueue", "cancel-enqueue", "payload://enqueue", DateTimeOffset.UtcNow);
        var cancelledOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new OperationCanceledException("Simulated enqueue cancellation."))).Options;
        await using (var cancelledDb = new WorkerDbContext(cancelledOptions))
        {
            var repository = new WorkerTaskRepository(cancelledDb);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.EnqueueAsync(task));
            Assert.Equal(EntityState.Detached, cancelledDb.Entry(task).State);
        }
        await using var verifyDb = new WorkerDbContext(baseOptions);
        Assert.Empty(await verifyDb.WorkerTasks.AsNoTracking().ToArrayAsync());
    }

    [Fact]
    public async Task ReplayCancellationDetachesAddedTaskAndHistoryAndPropagatesCancellation()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid originalId;
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            var original = WorkerTask.Create(tenantId, "test.replay", "dead-lettered", "payload://replay", now, maxAttempts: 1);
            var leaseId = Guid.NewGuid();
            original.AcquireLease("worker", leaseId, now, now.AddMinutes(1));
            original.Start(leaseId, original.FencingToken, now);
            original.Fail(leaseId, original.FencingToken, "test.failed", now, now.AddMinutes(2));
            setup.WorkerTasks.Add(original);
            await setup.SaveChangesAsync();
            originalId = original.Id;
        }
        var cancelledOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new OperationCanceledException("Simulated replay cancellation."))).Options;
        await using (var cancelledDb = new WorkerDbContext(cancelledOptions))
        {
            var repository = new WorkerTaskRepository(cancelledDb);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.ReplayAsync(
                tenantId, originalId, Guid.NewGuid(), "operator:test", now.AddMinutes(3)));
            Assert.DoesNotContain(cancelledDb.ChangeTracker.Entries(), entry => entry.State == EntityState.Added
                && (entry.Entity is WorkerTask || entry.Entity is WorkerTaskReplayRecord));
        }
        await using var verifyDb = new WorkerDbContext(baseOptions);
        Assert.Single(await verifyDb.WorkerTasks.AsNoTracking().ToArrayAsync());
        Assert.Empty(await verifyDb.WorkerTaskReplayRecords.AsNoTracking().ToArrayAsync());
    }


    [Fact]
    public async Task EnqueuePersistenceFailureDetachesAddedTaskAndPropagatesFailure()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using (var setup = new WorkerDbContext(baseOptions)) await setup.Database.MigrateAsync();
        var task = WorkerTask.Create(Guid.NewGuid(), "test.enqueue", "failed-enqueue", "payload://enqueue", DateTimeOffset.UtcNow);
        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new IOException("Simulated enqueue failure."))).Options;

        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);
            await Assert.ThrowsAsync<IOException>(() => repository.EnqueueAsync(task));
            Assert.Equal(EntityState.Detached, failingDb.Entry(task).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        Assert.Empty(await verifyDb.WorkerTasks.AsNoTracking().ToArrayAsync());
    }

    [Fact]
    public async Task ReplayPersistenceFailureDetachesAddedTaskAndHistoryAndPropagatesFailure()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid originalId;
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            var original = WorkerTask.Create(tenantId, "test.replay", "dead-lettered-failure", "payload://replay", now, maxAttempts: 1);
            var leaseId = Guid.NewGuid();
            original.AcquireLease("worker", leaseId, now, now.AddMinutes(1));
            original.Start(leaseId, original.FencingToken, now);
            original.Fail(leaseId, original.FencingToken, "test.failed", now, now.AddMinutes(2));
            setup.WorkerTasks.Add(original);
            await setup.SaveChangesAsync();
            originalId = original.Id;
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new IOException("Simulated replay failure."))).Options;
        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);
            await Assert.ThrowsAsync<IOException>(() => repository.ReplayAsync(
                tenantId, originalId, Guid.NewGuid(), "operator:test", now.AddMinutes(3)));
            Assert.DoesNotContain(failingDb.ChangeTracker.Entries(), entry => entry.State == EntityState.Added
                && (entry.Entity is WorkerTask || entry.Entity is WorkerTaskReplayRecord));
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        Assert.Single(await verifyDb.WorkerTasks.AsNoTracking().ToArrayAsync());
        Assert.Empty(await verifyDb.WorkerTaskReplayRecords.AsNoTracking().ToArrayAsync());
    }

    [Fact]
    public async Task ReplayPersistenceFailureKeepsUnrelatedAddedEntitiesTracked()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid originalId;
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            var original = WorkerTask.Create(tenantId, "test.replay", "dead-lettered-unrelated", "payload://replay", now, maxAttempts: 1);
            var leaseId = Guid.NewGuid();
            original.AcquireLease("worker", leaseId, now, now.AddMinutes(1));
            original.Start(leaseId, original.FencingToken, now);
            original.Fail(leaseId, original.FencingToken, "test.failed", now, now.AddMinutes(2));
            setup.WorkerTasks.Add(original);
            await setup.SaveChangesAsync();
            originalId = original.Id;
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new IOException("Simulated replay failure."))).Options;
        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);
            var unrelated = WorkerTask.Create(tenantId, "test.unrelated", "unrelated-added", "payload://unrelated", now);
            failingDb.WorkerTasks.Add(unrelated);

            await Assert.ThrowsAsync<IOException>(() => repository.ReplayAsync(
                tenantId, originalId, Guid.NewGuid(), "operator:test", now.AddMinutes(3)));

            Assert.Equal(EntityState.Added, failingDb.Entry(unrelated).State);
            Assert.Single(failingDb.ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Added && entry.Entity is WorkerTask));
            Assert.DoesNotContain(failingDb.ChangeTracker.Entries(), entry =>
                entry.State == EntityState.Added && entry.Entity is WorkerTaskReplayRecord);
        }
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryPersistenceFailureDetachesTaskAndPreservesDatabaseState()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid taskId;
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            var task = WorkerTask.Create(tenantId, "test.recovery", "recovery-failure", "payload://recovery", now);
            var leaseId = Guid.NewGuid();
            task.AcquireLease("worker", leaseId, now, now.AddMinutes(1));
            task.Start(leaseId, task.FencingToken, now);
            setup.WorkerTasks.Add(task);
            await setup.SaveChangesAsync();
            taskId = task.Id;
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new IOException("Simulated recovery failure."))).Options;
        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);
            var tracked = await repository.GetAsync(tenantId, taskId);
            Assert.NotNull(tracked);
            await Assert.ThrowsAsync<IOException>(() => repository.RecoverExpiredLeasesAsync(now.AddMinutes(2), 10));
            Assert.Equal(EntityState.Detached, failingDb.Entry(tracked).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().SingleAsync(x => x.Id == taskId);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Status);
        Assert.NotNull(persisted.LeaseId);
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryCancellationDetachesTaskAndPropagatesCancellation()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid taskId;
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            var task = WorkerTask.Create(tenantId, "test.recovery", "recovery-cancel", "payload://recovery", now);
            var leaseId = Guid.NewGuid();
            task.AcquireLease("worker", leaseId, now, now.AddMinutes(1));
            task.Start(leaseId, task.FencingToken, now);
            setup.WorkerTasks.Add(task);
            await setup.SaveChangesAsync();
            taskId = task.Id;
        }

        var cancelledOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new OperationCanceledException("Simulated recovery cancellation."))).Options;
        await using (var cancelledDb = new WorkerDbContext(cancelledOptions))
        {
            var repository = new WorkerTaskRepository(cancelledDb);
            var tracked = await repository.GetAsync(tenantId, taskId);
            Assert.NotNull(tracked);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                repository.RecoverExpiredLeasesAsync(now.AddMinutes(2), 10));
            Assert.Equal(EntityState.Detached, cancelledDb.Entry(tracked).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().SingleAsync(x => x.Id == taskId);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Status);
        Assert.NotNull(persisted.LeaseId);
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryFailureAfterFirstSaveKeepsEarlierRecoveryAndStopsBatch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var taskIds = new List<Guid>();
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            for (var index = 0; index < 3; index++)
            {
                var task = WorkerTask.Create(tenantId, "test.recovery", $"batch-{index}", $"payload://{index}", now);
                var leaseId = Guid.NewGuid();
                task.AcquireLease("worker", leaseId, now, now.AddMinutes(index + 1));
                task.Start(leaseId, task.FencingToken, now);
                setup.WorkerTasks.Add(task);
                taskIds.Add(task.Id);
            }
            await setup.SaveChangesAsync();
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailOnSaveChangesNumberInterceptor(2, new IOException("Second recovery save failed.")))
            .Options;
        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);
            var second = await repository.GetAsync(tenantId, taskIds[1]);
            Assert.NotNull(second);

            await Assert.ThrowsAsync<IOException>(() => repository.RecoverExpiredLeasesAsync(now.AddMinutes(5), 3));

            Assert.Equal(EntityState.Detached, failingDb.Entry(second).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().OrderBy(x => x.IdempotencyKey).ToArrayAsync();
        Assert.Equal(WorkerTaskStatus.RetryScheduled, persisted.Single(x => x.Id == taskIds[0]).Status);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Single(x => x.Id == taskIds[1]).Status);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Single(x => x.Id == taskIds[2]).Status);
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryCancellationAfterFirstSaveKeepsEarlierRecoveryAndStopsBatch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var taskIds = new List<Guid>();

        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            for (var index = 0; index < 3; index++)
            {
                var task = WorkerTask.Create(tenantId, "test.recovery", $"cancel-batch-{index}", $"payload://{index}", now);
                var leaseId = Guid.NewGuid();
                task.AcquireLease("worker", leaseId, now, now.AddMinutes(index + 1));
                task.Start(leaseId, task.FencingToken, now);
                setup.WorkerTasks.Add(task);
                taskIds.Add(task.Id);
            }
            await setup.SaveChangesAsync();
        }

        var cancelledOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailOnSaveChangesNumberInterceptor(
                2, new OperationCanceledException("Second recovery save was cancelled.")))
            .Options;

        await using (var cancelledDb = new WorkerDbContext(cancelledOptions))
        {
            var repository = new WorkerTaskRepository(cancelledDb);
            var second = await repository.GetAsync(tenantId, taskIds[1]);
            Assert.NotNull(second);

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                repository.RecoverExpiredLeasesAsync(now.AddMinutes(5), 3));

            Assert.Equal(EntityState.Detached, cancelledDb.Entry(second).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().ToArrayAsync();
        Assert.Equal(WorkerTaskStatus.RetryScheduled, persisted.Single(x => x.Id == taskIds[0]).Status);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Single(x => x.Id == taskIds[1]).Status);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Single(x => x.Id == taskIds[2]).Status);
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryIgnoresLeasesThatHaveNotExpired()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid taskId;
        Guid leaseId;
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            var task = WorkerTask.Create(tenantId, "test.recovery", "not-expired", "payload://not-expired", now);
            leaseId = Guid.NewGuid();
            task.AcquireLease("worker", leaseId, now, now.AddMinutes(5));
            task.Start(leaseId, task.FencingToken, now);
            setup.WorkerTasks.Add(task);
            await setup.SaveChangesAsync();
            taskId = task.Id;
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailingSaveChangesInterceptor(new IOException("Recovery must not save an unexpired lease.")))
            .Options;
        await using (var failingDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(failingDb);

            var recovered = await repository.RecoverExpiredLeasesAsync(now.AddMinutes(1), 10);

            Assert.Equal(0, recovered);
            Assert.Empty(failingDb.ChangeTracker.Entries());
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().SingleAsync(x => x.Id == taskId);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Status);
        Assert.Equal(leaseId, persisted.LeaseId);
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryRejectsUnrelatedPendingChanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        Guid expiredTaskId;
        await using (var setup = new WorkerDbContext(options))
        {
            await setup.Database.MigrateAsync();
            var expiredTask = WorkerTask.Create(tenantId, "test.recovery", "expired-before-pending", "payload://expired", now);
            var leaseId = Guid.NewGuid();
            expiredTask.AcquireLease("worker", leaseId, now, now.AddMinutes(1));
            expiredTask.Start(leaseId, expiredTask.FencingToken, now);
            setup.WorkerTasks.Add(expiredTask);
            await setup.SaveChangesAsync();
            expiredTaskId = expiredTask.Id;
        }

        await using (var db = new WorkerDbContext(options))
        {
            var repository = new WorkerTaskRepository(db);
            var unrelatedTask = WorkerTask.Create(
                tenantId, "test.pending", "unrelated-pending", "payload://unrelated", now);
            db.WorkerTasks.Add(unrelatedTask);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.RecoverExpiredLeasesAsync(now.AddMinutes(5), 10));

            Assert.Equal(EntityState.Added, db.Entry(unrelatedTask).State);
        }

        await using var verifyDb = new WorkerDbContext(options);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().SingleAsync(x => x.Id == expiredTaskId);
        Assert.Equal(WorkerTaskStatus.Running, persisted.Status);
        Assert.Equal(1, await verifyDb.WorkerTasks.CountAsync(x => x.IdempotencyKey == "expired-before-pending"));
        Assert.Equal(0, await verifyDb.WorkerTasks.CountAsync(x => x.IdempotencyKey == "unrelated-pending"));
    }

    [Fact]
    public async Task ExpiredLeaseRecoveryConcurrencyConflictDetachesTaskAndContinuesBatch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var taskIds = new List<Guid>();
        await using (var setup = new WorkerDbContext(baseOptions))
        {
            await setup.Database.MigrateAsync();
            for (var index = 0; index < 2; index++)
            {
                var task = WorkerTask.Create(tenantId, "test.recovery", $"concurrency-{index}", $"payload://{index}", now);
                var leaseId = Guid.NewGuid();
                task.AcquireLease("worker", leaseId, now, now.AddMinutes(index + 1));
                task.Start(leaseId, task.FencingToken, now);
                setup.WorkerTasks.Add(task);
                taskIds.Add(task.Id);
            }
            await setup.SaveChangesAsync();
        }

        var concurrencyOptions = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection)
            .AddInterceptors(new FailOnSaveChangesNumberInterceptor(
                1, new DbUpdateConcurrencyException("Simulated recovery concurrency conflict.")))
            .Options;
        await using (var concurrencyDb = new WorkerDbContext(concurrencyOptions))
        {
            var repository = new WorkerTaskRepository(concurrencyDb);
            var conflicted = await repository.GetAsync(tenantId, taskIds[0]);
            Assert.NotNull(conflicted);

            var recovered = await repository.RecoverExpiredLeasesAsync(now.AddMinutes(5), 2);

            Assert.Equal(1, recovered);
            Assert.Equal(EntityState.Detached, concurrencyDb.Entry(conflicted).State);
        }

        await using var verifyDb = new WorkerDbContext(baseOptions);
        var persisted = await verifyDb.WorkerTasks.AsNoTracking().ToArrayAsync();
        Assert.Equal(WorkerTaskStatus.Running, persisted.Single(x => x.Id == taskIds[0]).Status);
        Assert.Equal(WorkerTaskStatus.RetryScheduled, persisted.Single(x => x.Id == taskIds[1]).Status);
    }

    private sealed class FailingSaveChangesInterceptor(Exception exception) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<InterceptionResult<int>>(exception);
    }

    private sealed class FailOnSaveChangesNumberInterceptor(int failOnSaveNumber, Exception exception) : SaveChangesInterceptor
    {
        private int _saveCount;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _saveCount++;
            return _saveCount == failOnSaveNumber
                ? ValueTask.FromException<InterceptionResult<int>>(exception)
                : ValueTask.FromResult(result);
        }
    }
}
