using EDAN.Tensor.Worker.Application.Execution;
using Xunit;

namespace EDAN.Tensor.Worker.Infrastructure.Tests;

public sealed class WorkerExecutionRegistryTests
{
    [Fact]
    public async Task ShutdownStopsNewEntriesAndWaitsForActiveExecutions()
    {
        var registry = new WorkerExecutionRegistry();
        Assert.True(registry.TryEnter(_ => Task.FromResult(true), out var execution));

        var shutdown = registry.StopAsync(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
        Assert.False(registry.TryEnter(_ => Task.FromResult(true), out _));

        execution!.Dispose();
        var result = await shutdown;

        Assert.True(result.DrainedWithinGracePeriod);
        Assert.Equal(0, result.HandoffRequested);
        Assert.Equal(0, result.StillRunning);
    }

    [Fact]
    public async Task ShutdownRequestsHandoffAfterGracePeriod()
    {
        var registry = new WorkerExecutionRegistry();
        WorkerExecutionLease? execution = null;
        var handoffStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(registry.TryEnter(_ =>
        {
            handoffStarted.TrySetResult();
            execution!.Dispose();
            return Task.FromResult(true);
        }, out execution));

        var result = await registry.StopAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(1));
        await handoffStarted.Task;

        Assert.False(result.DrainedWithinGracePeriod);
        Assert.Equal(1, result.HandoffRequested);
        Assert.Equal(1, result.HandoffSucceeded);
        Assert.Equal(0, result.StillRunning);
    }

    [Fact]
    public async Task FailedHandoffDoesNotReleaseExecutionLease()
    {
        var registry = new WorkerExecutionRegistry();
        Assert.True(registry.TryEnter(_ => Task.FromResult(false), out var execution));

        var result = await registry.StopAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(1));

        Assert.Equal(1, result.HandoffRequested);
        Assert.Equal(0, result.HandoffSucceeded);
        Assert.Equal(1, result.StillRunning);
        execution!.Dispose();
    }
}
