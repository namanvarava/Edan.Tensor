namespace EDAN.Tensor.Worker.Application.Execution;

public sealed class WorkerExecutionRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<long, WorkerExecutionLease> _active = [];
    private TaskCompletionSource _drained = CompletedSource();
    private long _nextId;
    private bool _accepting = true;

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

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(handoffTimeout);
        var outcomes = await Task.WhenAll(active.Select(async execution =>
        {
            try
            {
                // 콜백이 CancellationToken을 무시해도 종료 조정기가 인계 제한 시간을 넘기지 않도록 대기 자체를 제한합니다.
                return await execution.RequestHandoffAsync(timeout.Token).WaitAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested) { return false; }
            catch { return false; }
        }));
        try { await _drained.Task.WaitAsync(handoffTimeout, cancellationToken); }
        catch (TimeoutException) { }
        int remaining;
        lock (_sync) remaining = _active.Count;
        return new WorkerShutdownResult(false, outcomes.Length, outcomes.Count(x => x), remaining);
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
    bool DrainedWithinGracePeriod, int HandoffRequested, int HandoffSucceeded, int StillRunning);
