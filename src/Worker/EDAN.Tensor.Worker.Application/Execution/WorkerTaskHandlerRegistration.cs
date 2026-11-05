namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>DI 구성 시 Worker 작업 유형과 해당 Handler 인스턴스를 연결합니다.</summary>
public sealed record WorkerTaskHandlerRegistration(string TaskType, IWorkerTaskHandler Handler, string? Description = null);
