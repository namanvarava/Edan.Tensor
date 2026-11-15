using EDAN.Tensor.Worker.Application.Abstractions;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>Tenant 관리자용 작업 취소·강제 재시도·결과 참조 조회를 조정합니다.</summary>
public sealed class WorkerTaskCommandService(IWorkerTaskRepository repository, TimeProvider timeProvider)
{
    public Task<bool> CancelAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default) =>
        repository.TryOperatorCancelAsync(tenantId, taskId, timeProvider.GetUtcNow(), cancellationToken);

    public Task<bool> ForceRetryAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default) =>
        repository.TryForceRetryAsync(tenantId, taskId, timeProvider.GetUtcNow(), cancellationToken);

    public async Task<WorkerTaskResultView?> GetResultAsync(Guid tenantId, Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(tenantId, taskId, cancellationToken);
        return task?.ResultReference is null
            ? null
            : new WorkerTaskResultView(task.Id, task.TaskType, task.Status.ToString(), task.ResultReference);
    }
}

/// <summary>Worker가 저장한 외부 결과 참조를 Tenant 경계 안에서 반환합니다.</summary>
public sealed record WorkerTaskResultView(Guid TaskId, string TaskType, string Status, string ResultReference);
