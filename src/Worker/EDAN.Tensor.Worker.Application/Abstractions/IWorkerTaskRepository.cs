using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>작업 저장소는 Tenant 경계를 모든 조회와 쓰기에 적용합니다.</summary>
public interface IWorkerTaskRepository
{
    Task<WorkerTask?> GetAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetReadyTenantIdsAsync(DateTimeOffset nowUtc, int take,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkerTask>> GetReadyAsync(Guid tenantId, DateTimeOffset nowUtc, int take,
        CancellationToken cancellationToken = default);
    Task<WorkerTask?> TryAcquireLeaseAsync(Guid tenantId, Guid taskId, string workerId, Guid leaseId,
        DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default);
    Task<bool> TryStartAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
    Task<bool> TrySaveCheckpointAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        string checkpoint, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
    Task<bool> TryRenewLeaseAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default);
    Task<bool> TryCompleteAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
    Task<bool> TryFailAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, string errorCode,
        DateTimeOffset nowUtc, DateTimeOffset retryAtUtc, CancellationToken cancellationToken = default);
    Task<bool> TryHandoffAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        string checkpoint, DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
    Task<WorkerTaskEnqueueResult> EnqueueAsync(WorkerTask task, CancellationToken cancellationToken = default);
    void Add(WorkerTask task);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
