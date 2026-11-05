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
            .AddInterceptors(new FailingSaveChangesInterceptor())
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

    private sealed class FailingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromException<InterceptionResult<int>>(new IOException("Simulated persistence failure."));
    }
}
