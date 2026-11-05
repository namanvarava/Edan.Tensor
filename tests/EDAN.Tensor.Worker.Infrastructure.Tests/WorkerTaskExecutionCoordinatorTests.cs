using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerTaskExecutionCoordinatorTests
{
    [Fact]
    public async Task CoordinatorRunsHandlerAndPersistsCompletion()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var task = WorkerTask.Create(tenantId, "test.success", "execution-key", "payload://test", DateTimeOffset.UtcNow);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var handler = new TestHandler();
        var coordinator = new WorkerTaskExecutionCoordinator(
            repository, new TestResolver(handler), new SuccessfulLeaseRenewer(),
            new WorkerExecutionRegistry(), TimeProvider.System);

        var outcome = await coordinator.ExecuteAsync(tenantId, task.Id, "worker-test", TimeSpan.FromMinutes(5));

        Assert.Equal(WorkerTaskExecutionOutcome.Succeeded, outcome);
        Assert.Equal(WorkerTaskStatus.Succeeded, (await repository.GetAsync(tenantId, task.Id))!.Status);
    }

    [Fact]
    public void HandlerRegistryResolvesRegisteredTypeAndRejectsDuplicates()
    {
        var registry = new WorkerTaskHandlerRegistry();
        var handler = new TestHandler();

        registry.Register("test.success", handler);

        Assert.Same(handler, registry.Resolve("test.success"));
        Assert.Null(registry.Resolve("test.unknown"));
        Assert.Throws<InvalidOperationException>(() => registry.Register("test.success", handler));
    }

    private sealed class SuccessfulLeaseRenewer : IWorkerTaskLeaseRenewer
    {
        public Task<bool> RenewAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
            DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class TestResolver(IWorkerTaskHandler handler) : IWorkerTaskHandlerResolver
    {
        public IWorkerTaskHandler? Resolve(string taskType) => taskType == "test.success" ? handler : null;
    }

    private sealed class TestHandler : IWorkerTaskHandler
    {
        public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken) =>
            Task.FromResult<IWorkerTaskExecution>(new TestExecution());
    }

    private sealed class TestExecution : IWorkerTaskExecution
    {
        public Task<WorkerTaskExecutionResult> Completion =>
            Task.FromResult(WorkerTaskExecutionResult.Success());
        public Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken) =>
            Task.FromResult<string?>("test:checkpoint");
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
