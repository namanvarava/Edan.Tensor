using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>Tenant 범위의 비동기 작업 등록과 Idempotency 계약을 조정합니다.</summary>
public sealed class WorkerTaskEnqueueService(
    IWorkerTaskRepository repository,
    IWorkerTaskTypeCatalog taskTypeCatalog,
    TimeProvider timeProvider)
{
    /// <summary>활성 카탈로그 유형만 등록하고 같은 Tenant/Key 요청은 안전하게 수렴시킵니다.</summary>
    public async Task<WorkerTaskEnqueueResult> EnqueueAsync(Guid tenantId, string idempotencyKey,
        string taskType, string payloadReference, int maxAttempts = 5,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (idempotencyKey.Trim().Length > 200)
            throw new ArgumentException("Idempotency Key는 200자를 초과할 수 없습니다.", nameof(idempotencyKey));
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);

        var normalizedTaskType = taskType.Trim();
        if (!await taskTypeCatalog.IsEnabledAsync(normalizedTaskType, cancellationToken))
            throw new WorkerTaskTypeNotAvailableException(normalizedTaskType);

        var task = WorkerTask.Create(tenantId, normalizedTaskType, idempotencyKey,
            payloadReference, timeProvider.GetUtcNow(), maxAttempts);
        return await repository.EnqueueAsync(task, cancellationToken);
    }
}
