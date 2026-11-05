using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>Tenant 범위의 비동기 작업 등록과 Idempotency 계약을 조정합니다.</summary>
public sealed class WorkerTaskEnqueueService(
    IWorkerTaskRepository repository,
    TimeProvider timeProvider)
{
    /// <summary>같은 Tenant와 Key의 동일 요청은 기존 작업을 반환하고 다른 요청 내용은 충돌로 처리합니다.</summary>
    public Task<WorkerTaskEnqueueResult> EnqueueAsync(Guid tenantId, string idempotencyKey,
        string taskType, string payloadReference, int maxAttempts = 5,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (idempotencyKey.Trim().Length > 200)
            throw new ArgumentException("Idempotency Key는 200자를 초과할 수 없습니다.", nameof(idempotencyKey));

        var task = WorkerTask.Create(tenantId, taskType, idempotencyKey,
            payloadReference, timeProvider.GetUtcNow(), maxAttempts);
        return repository.EnqueueAsync(task, cancellationToken);
    }
}
