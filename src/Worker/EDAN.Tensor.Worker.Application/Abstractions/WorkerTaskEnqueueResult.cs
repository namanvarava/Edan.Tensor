namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>Idempotent 작업 등록 결과입니다. 요청 내용이 다르면 Conflict로 구분합니다.</summary>
public sealed record WorkerTaskEnqueueResult(EDAN.Tensor.Worker.Domain.Model.WorkerTask Task,
    bool Created, bool Conflict);
