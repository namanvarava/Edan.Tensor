using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>작업의 안전한 운영 상태를 Tenant 범위로 조회합니다.</summary>
public sealed class WorkerTaskQueryService(IWorkerTaskRepository repository)
{
    /// <summary>Payload와 체크포인트 본문을 제외한 작업 상태를 반환합니다.</summary>
    public async Task<WorkerTaskStatusView?> GetAsync(Guid tenantId, Guid taskId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        if (taskId == Guid.Empty)
            throw new ArgumentException("작업 식별자는 비어 있을 수 없습니다.", nameof(taskId));

        var task = await repository.GetAsync(tenantId, taskId, cancellationToken);
        return task is null ? null : new WorkerTaskStatusView(
            task.Id, task.TaskType, task.Status.ToString(), task.AttemptCount,
            task.MaxAttempts, task.FencingToken, task.CheckpointVersion,
            task.CreatedAtUtc, task.NextAttemptAtUtc, task.CompletedAtUtc,
            task.LastErrorCode, task.LeaseExpiresAtUtc, task.ProgressPercent,\n            task.ProgressStage, task.ProgressMessage, task.ResultReference is not null);
    }
}

/// <summary>API에 공개 가능한 작업 운영 메타데이터입니다.</summary>
public sealed record WorkerTaskStatusView(
    Guid Id,
    string TaskType,
    string Status,
    int AttemptCount,
    int MaxAttempts,
    long FencingToken,
    long CheckpointVersion,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? LastErrorCode,
    DateTimeOffset? LeaseExpiresAtUtc,\n    int ProgressPercent,\n    string? ProgressStage,\n    string? ProgressMessage,\n    bool HasResult);
