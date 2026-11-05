using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>Worker 작업 저장소는 모든 조회와 쓰기에서 Tenant 경계를 적용합니다.</summary>
public interface IWorkerTaskRepository
{
    Task<WorkerTask?> GetAsync(TenantId tenantId, Guid taskId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkerTask>> GetReadyAsync(TenantId tenantId, int take,
        CancellationToken cancellationToken = default);
    void Add(WorkerTask task);
}
