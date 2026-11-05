using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>DLQ 조회와 이력 보존형 재처리를 제공합니다.</summary>
public interface IWorkerTaskDeadLetterRepository
{
    Task<IReadOnlyList<WorkerTask>> GetDeadLetteredAsync(Guid tenantId, int skip, int take,
        CancellationToken cancellationToken = default);
    Task<Guid?> ReplayAsync(Guid tenantId, Guid taskId, string requestedBy, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}
