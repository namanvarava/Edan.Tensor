using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>Tenant 관리자용 작업 취소·강제 재시도·결과 참조 조회를 조정합니다.</summary>
public sealed class WorkerTaskCommandService(IWorkerTaskRepository repository, IWorkerResultStore resultStore, TimeProvider timeProvider)
{
    public Task<bool> CancelAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default) =>
        repository.TryOperatorCancelAsync(tenantId, taskId, timeProvider.GetUtcNow(), cancellationToken);

    public Task<bool> ForceRetryAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default) =>
        repository.TryForceRetryAsync(tenantId, taskId, timeProvider.GetUtcNow(), cancellationToken);

    public async Task<WorkerTaskResultView?> GetResultAsync(Guid tenantId, Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(tenantId, taskId, cancellationToken);
        if (task?.ResultReference is null)
            return null;
        var metadata = await resultStore.GetMetadataAsync(tenantId, taskId, task.ResultReference, cancellationToken);
        return new WorkerTaskResultView(task.Id, task.TaskType, task.Status.ToString(), task.ResultReference, metadata);
    }

    public async Task<WorkerRawResult?> GetRawResultAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(tenantId, taskId, cancellationToken);
        if (task?.ResultReference is null) return null;
        var metadata = await resultStore.GetMetadataAsync(tenantId, taskId, task.ResultReference, cancellationToken);
        if (metadata is null) return null;
        var stream = await resultStore.OpenReadAsync(metadata, cancellationToken);
        return stream is null ? null : new WorkerRawResult(metadata, stream);
    }

    public async Task<WorkerTaskResultView?> StoreRawResultAsync(Guid tenantId, Guid taskId, string contentType,
        Stream content, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetAsync(tenantId, taskId, cancellationToken);
        if (task is null || task.Status != WorkerTaskStatus.Succeeded) return null;
        var metadata = await resultStore.SaveAsync(tenantId, taskId, contentType, content, cancellationToken);
        if (!await repository.TryAttachResultReferenceAsync(tenantId, taskId, metadata.ResultReference, cancellationToken))
        {
            await resultStore.DeleteAsync(metadata, cancellationToken);
            return null;
        }
        return new WorkerTaskResultView(taskId, task.TaskType, task.Status.ToString(), metadata.ResultReference, metadata);
    }
}

/// <summary>Worker가 저장한 외부 결과 참조를 Tenant 경계 안에서 반환합니다.</summary>
public sealed record WorkerTaskResultView(Guid TaskId, string TaskType, string Status, string ResultReference, WorkerResultMetadata? Metadata = null);
public sealed record WorkerRawResult(WorkerResultMetadata Metadata, Stream Content);
