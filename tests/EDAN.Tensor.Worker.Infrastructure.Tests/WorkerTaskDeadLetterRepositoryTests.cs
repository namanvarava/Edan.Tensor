using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskDeadLetterRepositoryTests
{
    [Fact]
    public async Task DeadLetterCanBeListedAndReplayedWithoutChangingOriginal()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var original = WorkerTask.Create(tenantId, "archive.generate", "original-key",
            "payload://archive/1", now, maxAttempts: 2);
        var lease = Guid.NewGuid();
        var token = original.AcquireLease("worker-a", lease, now, now.AddMinutes(5));
        original.Start(lease, token, now);
        original.Fail(lease, token, "archive.storage.failed", now.AddSeconds(1), now.AddMinutes(1));
        var retryLease = Guid.NewGuid();
        var retryNow = now.AddMinutes(2);
        var retryToken = original.AcquireLease("worker-b", retryLease, retryNow, retryNow.AddMinutes(5));
        original.Fail(retryLease, retryToken, "archive.storage.failed", retryNow.AddSeconds(1), retryNow.AddMinutes(2));
        db.WorkerTasks.Add(original);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var service = new WorkerTaskDeadLetterService(repository, TimeProvider.System);
        var listed = await service.GetAsync(tenantId);
        Assert.Single(listed);
        Assert.Equal(original.Id, listed[0].Id);

        var replayRequestId = Guid.NewGuid();
        var replayId = await service.ReplayAsync(tenantId, original.Id, replayRequestId, "operator:test");
        var repeatedReplayId = await service.ReplayAsync(tenantId, original.Id, replayRequestId, "operator:test");
        Assert.NotNull(replayId);
        Assert.Equal(replayId, repeatedReplayId);

        var unchangedOriginal = await repository.GetAsync(tenantId, original.Id);
        var replay = await repository.GetAsync(tenantId, replayId!.Value);
        var history = await db.WorkerTaskReplayRecords.SingleAsync();

        Assert.Equal(WorkerTaskStatus.DeadLettered, unchangedOriginal!.Status);
        Assert.Equal(2, unchangedOriginal.AttemptCount);
        Assert.Equal(WorkerTaskStatus.Pending, replay!.Status);
        Assert.Equal(0, replay.AttemptCount);
        Assert.Equal(original.TaskType, replay.TaskType);
        Assert.Equal(original.PayloadReference, replay.PayloadReference);
        Assert.Equal(original.MaxAttempts, replay.MaxAttempts);
        Assert.Equal(original.Id, history.OriginalTaskId);
        Assert.Equal(replay.Id, history.ReplayTaskId);
        Assert.Equal(replayRequestId, history.ReplayRequestId);
        Assert.Equal("operator:test", history.RequestedBy);
        Assert.Single(await db.WorkerTaskReplayRecords.ToArrayAsync());
    }

    [Fact]
    public async Task ReplayCannotAccessAnotherTenantOrNonDeadLetterTask()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();
        var tenantId = Guid.NewGuid();
        var task = WorkerTask.Create(tenantId, "archive.generate", "pending-key",
            "payload://archive/2", DateTimeOffset.UtcNow);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        Assert.Null(await repository.ReplayAsync(Guid.NewGuid(), task.Id, Guid.NewGuid(), "operator:test", DateTimeOffset.UtcNow));
        Assert.Null(await repository.ReplayAsync(tenantId, task.Id, Guid.NewGuid(), "operator:test", DateTimeOffset.UtcNow));
        Assert.Empty(await repository.GetDeadLetteredAsync(Guid.NewGuid(), 0, 10));
        Assert.Empty(await db.WorkerTaskReplayRecords.ToArrayAsync());
    }
}
