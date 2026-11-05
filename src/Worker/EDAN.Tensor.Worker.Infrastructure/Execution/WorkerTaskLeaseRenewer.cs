using EDAN.Tensor.Worker.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.Worker.Infrastructure.Execution;

/// <summary>Lease 갱신마다 독립 DI 범위와 DbContext를 사용해 실행 작업과의 동시 접근을 방지합니다.</summary>
public sealed class WorkerTaskLeaseRenewer(IServiceScopeFactory scopeFactory) : IWorkerTaskLeaseRenewer
{
    public async Task<bool> RenewAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWorkerTaskRepository>();
        return await repository.TryRenewLeaseAsync(
            tenantId, taskId, leaseId, fencingToken, nowUtc, expiresAtUtc, cancellationToken);
    }
}
