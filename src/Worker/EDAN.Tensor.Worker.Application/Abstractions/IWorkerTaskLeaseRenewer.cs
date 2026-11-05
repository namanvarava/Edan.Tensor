namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>별도 저장소 범위에서 실행 Lease를 갱신합니다.</summary>
public interface IWorkerTaskLeaseRenewer
{
    Task<bool> RenewAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default);
}
