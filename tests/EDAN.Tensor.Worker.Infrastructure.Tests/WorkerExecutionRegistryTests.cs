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
        Assert.Equal(TimeSpan.Zero, result.HandoffElapsed);
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
        Assert.True(result.HandoffElapsed >= TimeSpan.Zero);
        Assert.True(result.HandoffElapsed < TimeSpan.FromSeconds(1));
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

    [Fact]
    public async Task ShutdownReturnsWhenHandoffCallbackIgnoresCancellation()
    {
        var registry = new WorkerExecutionRegistry();
        var handoffStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseHandoff = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handoffFinished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Assert.True(registry.TryEnter(async _ =>
        {
            handoffStarted.TrySetResult();
            try
            {
                // 의도적으로 전달된 취소 토큰을 사용하지 않는 콜백을 시뮬레이션합니다.
                await releaseHandoff.Task;
                return true;
            }
            finally
            {
                handoffFinished.TrySetResult();
            }
        }, out var execution));

        var shutdown = registry.StopAsync(TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(250));
        await handoffStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        var result = await shutdown.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.False(result.DrainedWithinGracePeriod);
        Assert.Equal(1, result.HandoffRequested);
        Assert.Equal(0, result.HandoffSucceeded);
        Assert.Equal(1, result.StillRunning);
        Assert.True(result.HandoffElapsed <= TimeSpan.FromMilliseconds(350));

        releaseHandoff.TrySetResult();
        await handoffFinished.Task.WaitAsync(TimeSpan.FromSeconds(1));
        execution!.Dispose();
    }

    [Fact]
    public async Task HandoffRequestAfterExecutionLeaseIsDisposedIsIgnored()
    {
        var registry = new WorkerExecutionRegistry();
        var calls = 0;
        Assert.True(registry.TryEnter(_ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(true);
        }, out var execution));

        execution!.Dispose();

        Assert.False(await execution.RequestHandoffAsync(CancellationToken.None));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task ConcurrentHandoffRequestsAreSerializedAndSuccessfulHandoffIsIdempotent()
    {
        var registry = new WorkerExecutionRegistry();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        Assert.True(registry.TryEnter(async _ =>
        {
            Interlocked.Increment(ref calls);
            started.TrySetResult();
            await release.Task;
            return true;
        }, out var execution));
        var first = execution!.RequestHandoffAsync(CancellationToken.None);
        await started.Task;
        var second = execution.RequestHandoffAsync(CancellationToken.None);
        release.TrySetResult();
        Assert.True(await first);
        Assert.True(await second);
        Assert.True(await execution.RequestHandoffAsync(CancellationToken.None));
        Assert.Equal(1, calls);
        execution.Dispose();
    }
}
