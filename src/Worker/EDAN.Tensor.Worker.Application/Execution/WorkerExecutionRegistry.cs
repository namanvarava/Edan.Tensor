namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>Worker 실행 진입을 통제하고 종료 시 실행 중 작업의 안전한 인계를 조정합니다.</summary>
public sealed class WorkerExecutionRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<long, WorkerExecutionLease> _active = [];
    private TaskCompletionSource _drained = CompletedSource();
    private long _nextId;
    private bool _accepting = true;

    /// <summary>실행 슬롯을 등록합니다. 종료가 시작된 뒤에는 새 슬롯을 허용하지 않습니다.</summary>
    public bool TryEnter(Func<CancellationToken, Task<bool>> requestHandoff, out WorkerExecutionLease? lease)
    {
        ArgumentNullException.ThrowIfNull(requestHandoff);
        lock (_sync)
        {
            if (!_accepting)
            {
                lease = null;
                return false;
            }

            if (_active.Count == 0)
                _drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var id = ++_nextId;
            lease = new WorkerExecutionLease(this, id, requestHandoff);
            _active.Add(id, lease);
            return true;
        }
    }

    /// <summary>새 실행을 차단하고 유예 시간 이후 남은 실행에 안전한 인계를 요청합니다.</summary>
    public async Task<WorkerShutdownResult> StopAsync(
        TimeSpan gracePeriod, TimeSpan handoffTimeout, CancellationToken cancellationToken = default)
    {
        if (gracePeriod < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(gracePeriod));
        if (handoffTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(handoffTimeout));

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
        catch (TimeoutException)
        {
            // 유예 시간이 끝난 실행에만 협력적 인계를 요청합니다.
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(handoffTimeout);
        var outcomes = await Task.WhenAll(active.Select(async execution =>
        {
            try
            {
                return await execution.RequestHandoffAsync(timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }));

        try
        {
            await _drained.Task.WaitAsync(handoffTimeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            // 남은 실행의 Lease는 임의로 해제하지 않고 만료 및 복구 절차에 맡깁니다.
        }

        int remaining;
        lock (_sync)
            remaining = _active.Count;

        return new WorkerShutdownResult(false, outcomes.Length, outcomes.Count(x => x), remaining);
    }

    internal void Exit(long id)
    {
        lock (_sync)
        {
            _active.Remove(id);
            if (_active.Count == 0)
                _drained.TrySetResult();
        }
    }

    private static TaskCompletionSource CompletedSource()
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        source.SetResult();
        return source;
    }
}

/// <summary>실행 중 작업의 종료 인계 경계를 나타냅니다.</summary>
public sealed class WorkerExecutionLease : IDisposable
{
    private readonly WorkerExecutionRegistry _owner;
    private readonly long _id;
    private readonly Func<CancellationToken, Task<bool>> _requestHandoff;
    private int _disposed;

    internal WorkerExecutionLease(
        WorkerExecutionRegistry owner, long id, Func<CancellationToken, Task<bool>> requestHandoff)
    {
        _owner = owner;
        _id = id;
        _requestHandoff = requestHandoff;
    }

    /// <summary>실행기가 안전한 체크포인트 경계에서 인계를 완료했는지 확인합니다.</summary>
    public Task<bool> RequestHandoffAsync(CancellationToken cancellationToken) =>
        _requestHandoff(cancellationToken);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _owner.Exit(_id);
    }
}

/// <summary>Worker 종료 시 실행 정리 결과입니다.</summary>
public sealed record WorkerShutdownResult(
    bool DrainedWithinGracePeriod, int HandoffRequested, int HandoffSucceeded, int StillRunning);
