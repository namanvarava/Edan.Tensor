using System.Diagnostics;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>
/// Registry는 실행 임대를 추적하고 신규 실행 차단, 정상 완료 대기 및 제한된 시간의 인계를 조정합니다.
/// 호스트는 작업 실행 전에 TryEnter를 호출하고 실행이 종료되면 반환된 임대를 반드시 해제해야 합니다.
/// </summary>
public sealed class WorkerExecutionRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<long, WorkerExecutionLease> _active = [];
    private TaskCompletionSource _drained = CompletedSource();
    private long _nextId;
    private bool _accepting = true;

    /// <summary>
    /// Registry는 종료가 시작되지 않은 경우 실행 임대와 인계 콜백을 등록합니다.
    /// 콜백 구현자는 전달된 취소 토큰을 준수하고 체크포인트와 Lease 변경을 영속화한 경우에만 true를 반환해야 합니다.
    /// 호출자는 반환값이 false이면 새 작업을 시작하지 않아야 하며, 등록된 임대는 실행 종료 시 Dispose해야 합니다.
    /// </summary>
    public bool TryEnter(Func<CancellationToken, Task<bool>> requestHandoff, out WorkerExecutionLease? lease)
    {
        ArgumentNullException.ThrowIfNull(requestHandoff);
        lock (_sync)
        {
            if (!_accepting) { lease = null; return false; }
            if (_active.Count == 0)
                _drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var id = ++_nextId;
            lease = new WorkerExecutionLease(this, id, requestHandoff);
            _active.Add(id, lease);
            return true;
        }
    }

    /// <summary>
    /// Registry는 신규 등록을 차단하고 gracePeriod 동안 정상 완료를 기다린 뒤 handoffTimeout을 공유하여 인계를 요청합니다.
    /// 콜백 호출은 별도 작업에서 실행되므로 콜백이 동기적으로 대기해도 종료 호출의 대기 시간은 제한됩니다.
    /// 시간 초과는 콜백의 강제 중단이나 Lease 해제를 의미하지 않으며, 인계 실패 작업은 영속 Lease 만료 복구에 맡겨야 합니다.
    /// 호출자는 HandoffElapsed를 차감한 나머지 예산만 후속 정리에 사용해야 합니다.
    /// 호출자가 cancellationToken을 취소하면 유예 또는 후속 정리 대기에서 OperationCanceledException이 전달될 수 있습니다.
    /// </summary>
    public async Task<WorkerShutdownResult> StopAsync(
        TimeSpan gracePeriod, TimeSpan handoffTimeout, CancellationToken cancellationToken = default)
    {
        if (gracePeriod < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(gracePeriod));
        if (handoffTimeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(handoffTimeout));
        WorkerExecutionLease[] active;
        Task drained;
        lock (_sync)
        {
            _accepting = false;
            active = _active.Values.ToArray();
            drained = _drained.Task;
        }
        try
        {
            await drained.WaitAsync(gracePeriod, cancellationToken);
            return new WorkerShutdownResult(true, 0, 0, 0);
        }
        catch (TimeoutException) { }

        var handoffStartedAt = Stopwatch.GetTimestamp();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(handoffTimeout);
        var outcomes = await Task.WhenAll(active.Select(async execution =>
        {
            try
            {
                // 종료 조정기는 콜백 호출을 별도 작업으로 실행하여 동기 대기와 비동기 대기를 모두 제한합니다.
                // 콜백 구현자는 취소 토큰을 준수해야 하며, 제한 시간 초과는 콜백 자체의 강제 중단을 보장하지 않습니다.
                return await Task.Run(() => execution.RequestHandoffAsync(timeout.Token), timeout.Token)
                    .WaitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested) { return false; }
            catch { return false; }
        }));
        var remainingHandoffTime = handoffTimeout - Stopwatch.GetElapsedTime(handoffStartedAt);
        if (remainingHandoffTime > TimeSpan.Zero)
        {
            try { await _drained.Task.WaitAsync(remainingHandoffTime, cancellationToken); }
            catch (TimeoutException) { }
        }
        var handoffElapsed = Stopwatch.GetElapsedTime(handoffStartedAt);
        int remaining;
        lock (_sync) remaining = _active.Count;
        return new WorkerShutdownResult(false, outcomes.Length, outcomes.Count(x => x), remaining, handoffElapsed);
    }

    internal void Exit(long id)
    {
        lock (_sync)
        {
            _active.Remove(id);
            if (_active.Count == 0) _drained.TrySetResult();
        }
    }

    private static TaskCompletionSource CompletedSource()
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        source.SetResult();
        return source;
    }
}

public sealed class WorkerExecutionLease : IDisposable
{
    private readonly WorkerExecutionRegistry _owner;
    private readonly long _id;
    private readonly Func<CancellationToken, Task<bool>> _requestHandoff;
    private readonly SemaphoreSlim _handoffGate = new(1, 1);
    private int _handoffSucceeded;
    private int _disposed;

    internal WorkerExecutionLease(WorkerExecutionRegistry owner, long id,
        Func<CancellationToken, Task<bool>> requestHandoff)
    {
        _owner = owner;
        _id = id;
        _requestHandoff = requestHandoff;
    }

    public async Task<bool> RequestHandoffAsync(CancellationToken cancellationToken)
    {
        if (Volatile.Read(ref _disposed) == 1) return false;
        if (Volatile.Read(ref _handoffSucceeded) == 1) return true;
        await _handoffGate.WaitAsync(cancellationToken);
        try
        {
            if (Volatile.Read(ref _disposed) == 1) return false;
            if (_handoffSucceeded == 1) return true;
            var succeeded = await _requestHandoff(cancellationToken);
            if (succeeded) Volatile.Write(ref _handoffSucceeded, 1);
            return succeeded;
        }
        finally { _handoffGate.Release(); }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0) _owner.Exit(_id);
    }
}

public sealed record WorkerShutdownResult(
    bool DrainedWithinGracePeriod,
    int HandoffRequested,
    int HandoffSucceeded,
    int StillRunning,
    TimeSpan HandoffElapsed = default);
