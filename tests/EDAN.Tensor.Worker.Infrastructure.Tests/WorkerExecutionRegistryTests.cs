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
        // HandoffElapsed에는 스케줄링 지연이 포함될 수 있으므로 호출자 대기 상한은 위 WaitAsync로 검증합니다.
        Assert.True(result.HandoffElapsed >= TimeSpan.Zero);

        releaseHandoff.TrySetResult();
        await handoffFinished.Task.WaitAsync(TimeSpan.FromSeconds(1));
        execution!.Dispose();
    }

    /// <summary>
    /// 테스트는 인계 콜백이 Task를 반환하기 전에 동기 대기하더라도 종료 조정기가 인계 시간 예산을 지키는지 검증합니다.
    /// 테스트는 종료 결과를 확인한 뒤 콜백을 해제하여 실행 스레드와 실행 임대가 남지 않도록 정리합니다.
    /// </summary>
    [Fact]
    public async Task ShutdownReturnsWhenHandoffCallbackBlocksBeforeReturningTask()
    {
        var registry = new WorkerExecutionRegistry();
        using var release = new ManualResetEventSlim();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Assert.True(registry.TryEnter(_ =>
        {
            started.TrySetResult();
            try
            {
                // 콜백은 취소 토큰을 무시하는 외부 동기 API의 정지 상태를 재현합니다.
                release.Wait();
                return Task.FromResult(true);
            }
            finally
            {
                finished.TrySetResult();
            }
        }, out var execution));

        Task<WorkerShutdownResult>? shutdown = null;
        try
        {
            // 테스트는 호출자 스레드의 정지까지 재현하므로 종료 호출 자체도 별도 작업에서 시작합니다.
            shutdown = Task.Run(() => registry.StopAsync(TimeSpan.Zero, TimeSpan.FromMilliseconds(250)));
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var result = await shutdown.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.False(result.DrainedWithinGracePeriod);
            Assert.Equal(1, result.HandoffRequested);
            Assert.Equal(0, result.HandoffSucceeded);
            Assert.Equal(1, result.StillRunning);
            Assert.InRange(result.HandoffElapsed, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }
        finally
        {
            release.Set();
            if (shutdown is not null)
                await shutdown.WaitAsync(TimeSpan.FromSeconds(5));
            await finished.Task.WaitAsync(TimeSpan.FromSeconds(5));
            execution!.Dispose();
        }
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
