using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskRepositoryPersistenceFailureTests
{
    [Fact]
    public async Task InvalidOperationExceptionFromPersistenceIsPropagatedAndFailedEntitiesAreDetached()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var baseOptions = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseSqlite(connection)
            .Options;

        var tenantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var pending = WorkerTask.Create(tenantId, "test.persistence", "acquire-failure",
            "payload://acquire", now);
        var running = WorkerTask.Create(tenantId, "test.persistence", "mutate-failure",
            "payload://mutate", now);
        var runningLeaseId = Guid.NewGuid();
        running.AcquireLease("worker", runningLeaseId, now, now.AddMinutes(5));
        running.Start(runningLeaseId, running.FencingToken, now);

        await using (var seedDb = new WorkerDbContext(baseOptions))
        {
            await seedDb.Database.MigrateAsync();
            seedDb.WorkerTasks.AddRange(pending, running);
            await seedDb.SaveChangesAsync();
        }

        var failingOptions = new DbContextOptionsBuilder<WorkerDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(new ThrowingSaveChangesInterceptor())
            .Options;

        await using (var acquireDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(acquireDb);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repository.TryAcquireLeaseAsync(tenantId, pending.Id, "worker-a",
                    Guid.NewGuid(), now, now.AddMinutes(1)));
            Assert.Empty(acquireDb.ChangeTracker.Entries<WorkerTask>());
        }

        await using (var mutateDb = new WorkerDbContext(failingOptions))
        {
            var repository = new WorkerTaskRepository(mutateDb);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repository.TryCompleteAsync(tenantId, running.Id, runningLeaseId,
                    running.FencingToken, now.AddMinutes(1)));
            Assert.Empty(mutateDb.ChangeTracker.Entries<WorkerTask>());
        }
    }

    private sealed class ThrowingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated persistence pipeline failure.");
    }
}
