namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>Worker가 지원하는 작업 유형의 영속 카탈로그 계약입니다.</summary>
public interface IWorkerTaskTypeCatalog
{
    Task<bool> IsEnabledAsync(string taskType, CancellationToken cancellationToken = default);
    Task SynchronizeAsync(IReadOnlyCollection<WorkerTaskTypeDefinition> definitions,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}

/// <summary>WorkerHost에 등록된 실행 가능 작업 유형의 공개 메타데이터입니다.</summary>
public sealed record WorkerTaskTypeDefinition(string TaskType, string? Description = null);

/// <summary>Worker 실행 구성이 지원하지 않는 작업 유형을 등록하려는 경우 발생합니다.</summary>
public sealed class WorkerTaskTypeNotAvailableException(string taskType)
    : InvalidOperationException($"Worker에서 지원하지 않는 작업 유형입니다: {taskType}")
{
    public string TaskType { get; } = taskType;
}
