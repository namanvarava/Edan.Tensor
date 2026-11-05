using EDAN.Tensor.Worker.Application.Abstractions;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>작업 유형과 실행 핸들러의 명시적 매핑을 관리합니다.</summary>
public sealed class WorkerTaskHandlerRegistry : IWorkerTaskHandlerResolver
{
    private readonly Dictionary<string, WorkerTaskHandlerRegistration> _handlers = new(StringComparer.Ordinal);
    private readonly object _sync = new();

    /// <summary>등록된 Handler 목록으로 레지스트리를 초기화합니다.</summary>
    public WorkerTaskHandlerRegistry(IEnumerable<WorkerTaskHandlerRegistration>? registrations = null)
    {
        foreach (var registration in registrations ?? [])
            Register(registration.TaskType, registration.Handler, registration.Description);
    }

    /// <summary>작업 유형에 실행 핸들러를 등록합니다. 중복 유형은 허용하지 않습니다.</summary>
    public void Register(string taskType, IWorkerTaskHandler handler, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);
        ArgumentNullException.ThrowIfNull(handler);
        var key = taskType.Trim();
        if (key.Length > 120)
            throw new ArgumentException("작업 유형은 120자를 초과할 수 없습니다.", nameof(taskType));
        lock (_sync)
        {
            if (!_handlers.TryAdd(key, new WorkerTaskHandlerRegistration(key, handler, description)))
                throw new InvalidOperationException($"작업 핸들러가 이미 등록되어 있습니다: {key}");
        }
    }

    /// <inheritdoc />
    public IWorkerTaskHandler? Resolve(string taskType)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            return null;
        lock (_sync)
            return _handlers.TryGetValue(taskType.Trim(), out var registration) ? registration.Handler : null;
    }

    /// <summary>Worker DB 카탈로그 동기화에 사용할 유형 정의를 반환합니다.</summary>
    public IReadOnlyList<WorkerTaskTypeDefinition> GetTaskTypes()
    {
        lock (_sync)
            return _handlers.Values
                .Select(x => new WorkerTaskTypeDefinition(x.TaskType, x.Description))
                .ToArray();
    }
}
