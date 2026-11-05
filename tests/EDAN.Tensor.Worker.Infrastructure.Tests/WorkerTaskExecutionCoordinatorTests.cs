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
    public async Task CoordinatorReportsDeadLetterWhenMaximumAttemptsAreReached()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var task = WorkerTask.Create(tenantId, "test.failure", "failure-key", "payload://test",
            DateTimeOffset.UtcNow, maxAttempts: 1);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var coordinator = new WorkerTaskExecutionCoordinator(
            repository, new FailureResolver(), new SuccessfulLeaseRenewer(),
            new WorkerExecutionRegistry(), TimeProvider.System);

        var outcome = await coordinator.ExecuteAsync(tenantId, task.Id, "worker-test", TimeSpan.FromMinutes(5));

        Assert.Equal(WorkerTaskExecutionOutcome.DeadLettered, outcome);
        var persisted = await repository.GetAsync(tenantId, task.Id);
        Assert.Equal(WorkerTaskStatus.DeadLettered, persisted!.Status);
        Assert.Equal(1, persisted.AttemptCount);
        Assert.Equal("test.permanent", persisted.LastErrorCode);
        Assert.Null(persisted.LeaseId);
        Assert.Null(persisted.NextAttemptAtUtc);
    }

    [Fact]
    public async Task CoordinatorSchedulesRetryWhenHandlerStartupThrows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var task = WorkerTask.Create(tenantId, "test.start-failure", "start-failure-key",
            "payload://test", DateTimeOffset.UtcNow, maxAttempts: 1);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        var repository = new WorkerTaskRepository(db);
        var coordinator = new WorkerTaskExecutionCoordinator(
            repository, new StartupFailureResolver(), new SuccessfulLeaseRenewer(),
            new WorkerExecutionRegistry(), TimeProvider.System);

        var outcome = await coordinator.ExecuteAsync(
            tenantId, task.Id, "worker-test", TimeSpan.FromMinutes(5));

        Assert.Equal(WorkerTaskExecutionOutcome.DeadLettered, outcome);
        var persisted = await repository.GetAsync(tenantId, task.Id);
        Assert.Equal(WorkerTaskStatus.DeadLettered, persisted!.Status);
        Assert.Equal("worker.handler.start_failed", persisted.LastErrorCode);
        Assert.Null(persisted.LeaseId);
    }

    [Fact]
    public async Task CoordinatorReleasesLeaseWhenHandlerStartupIsCancelled()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<WorkerDbContext>().UseSqlite(connection).Options;
        await using var db = new WorkerDbContext(options);
        await db.Database.MigrateAsync();

        var tenantId = Guid.NewGuid();
        var task = WorkerTask.Create(tenantId, "test.start-cancelled", "start-cancelled-key",
            "payload://test", DateTimeOffset.UtcNow, maxAttempts: 3);
        db.WorkerTasks.Add(task);
        await db.SaveChangesAsync();

        using var cancellation = new CancellationTokenSource();
        var repository = new WorkerTaskRepository(db);
        var coordinator = new WorkerTaskExecutionCoordinator(
            repository, new StartupCancellationResolver(cancellation), new SuccessfulLeaseRenewer(),
            new WorkerExecutionRegistry(), TimeProvider.System);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            coordinator.ExecuteAsync(tenantId, task.Id, "worker-test",
                TimeSpan.FromMinutes(5), cancellation.Token));

        var persisted = await repository.GetAsync(tenantId, task.Id);
        Assert.Equal(WorkerTaskStatus.RetryScheduled, persisted!.Status);
        Assert.Equal("worker.handler.start_cancelled", persisted.LastErrorCode);
        Assert.Null(persisted.LeaseId);
        Assert.Null(persisted.LeaseExpiresAtUtc);
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

    private sealed class StartupCancellationResolver(CancellationTokenSource cancellation)
        : IWorkerTaskHandlerResolver
    {
        public IWorkerTaskHandler? Resolve(string taskType) =>
            taskType == "test.start-cancelled"
                ? new StartupCancellationHandler(cancellation)
                : null;
    }

    private sealed class StartupCancellationHandler(CancellationTokenSource cancellation)
        : IWorkerTaskHandler
    {
        public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken)
        {
            cancellation.Cancel();
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private sealed class StartupFailureResolver : IWorkerTaskHandlerResolver
    {
        public IWorkerTaskHandler? Resolve(string taskType) =>
            taskType == "test.start-failure" ? new StartupFailureHandler() : null;
    }

    private sealed class StartupFailureHandler : IWorkerTaskHandler
    {
        public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Simulated handler startup failure.");
    }

    private sealed class FailureResolver : IWorkerTaskHandlerResolver
    {
        public IWorkerTaskHandler? Resolve(string taskType) =>
            taskType == "test.failure" ? new FailureHandler() : null;
    }

    private sealed class FailureHandler : IWorkerTaskHandler
    {
        public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken) =>
            Task.FromResult<IWorkerTaskExecution>(new FailureExecution());
    }

    private sealed class FailureExecution : IWorkerTaskExecution
    {
        public Task<WorkerTaskExecutionResult> Completion =>
            Task.FromResult(WorkerTaskExecutionResult.Failure("test.permanent", DateTimeOffset.UtcNow.AddMinutes(1)));
        public Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken) =>
            Task.FromResult<string?>("test:checkpoint");
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
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
