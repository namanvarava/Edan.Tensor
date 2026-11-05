namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>작업 유형과 실행 핸들러의 명시적 매핑을 관리합니다.</summary>
public sealed class WorkerTaskHandlerRegistry : IWorkerTaskHandlerResolver
{
    private readonly Dictionary<string, IWorkerTaskHandler> _handlers = new(StringComparer.Ordinal);
    private readonly object _sync = new();

    /// <summary>작업 유형에 실행 핸들러를 등록합니다. 중복 유형은 허용하지 않습니다.</summary>
    public void Register(string taskType, IWorkerTaskHandler handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);
        ArgumentNullException.ThrowIfNull(handler);
        var key = taskType.Trim();
        lock (_sync)
        {
            if (!_handlers.TryAdd(key, handler))
                throw new InvalidOperationException($"작업 핸들러가 이미 등록되어 있습니다: {key}");
        }
    }

    /// <inheritdoc />
    public IWorkerTaskHandler? Resolve(string taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            return null;
        lock (_sync)
            return _handlers.GetValueOrDefault(taskType.Trim());
    }
}
