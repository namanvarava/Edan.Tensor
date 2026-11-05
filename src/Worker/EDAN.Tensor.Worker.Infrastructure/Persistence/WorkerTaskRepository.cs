using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업을 Tenant 범위로 조회하고 실행 상태를 동시성 검증과 함께 영속화합니다.</summary>
public sealed class WorkerTaskRepository(WorkerDbContext db) : IWorkerTaskRepository
{
    public Task<WorkerTask?> GetAsync(Guid tenantId, Guid taskId,
        CancellationToken cancellationToken = default) =>
        db.WorkerTasks.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == taskId, cancellationToken);

    public async Task<IReadOnlyList<WorkerTask>> GetReadyAsync(Guid tenantId, DateTimeOffset nowUtc, int take,
        CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 500)
            throw new ArgumentOutOfRangeException(nameof(take), "조회 건수는 1~500이어야 합니다.");
        var tasks = await db.WorkerTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId &&
                (x.Status == WorkerTaskStatus.Pending || x.Status == WorkerTaskStatus.RetryScheduled ||
                 x.Status == WorkerTaskStatus.Checkpointed))
            .ToListAsync(cancellationToken);
        return tasks
            .Where(x => !x.NextAttemptAtUtc.HasValue || x.NextAttemptAtUtc.Value <= nowUtc.ToUniversalTime())
            .OrderBy(x => x.NextAttemptAtUtc ?? x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Take(take)
            .ToArray();
    }

    /// <summary>Tenant와 작업 상태를 확인하고 낙관적 동시성 검사를 거쳐 임대를 획득합니다.</summary>
    public async Task<WorkerTask?> TryAcquireLeaseAsync(Guid tenantId, Guid taskId, string workerId,
        Guid leaseId, DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var task = await GetAsync(tenantId, taskId, cancellationToken);
        if (task is null)
            return null;
        try
        {
            task.AcquireLease(workerId, leaseId, nowUtc, expiresAtUtc);
            await db.SaveChangesAsync(cancellationToken);
            return task;
        }
        catch (InvalidOperationException)
        {
            db.Entry(task).State = EntityState.Detached;
            return null;
        }
        catch (DbUpdateConcurrencyException)
        {
            db.Entry(task).State = EntityState.Detached;
            return null;
        }
    }

    public Task<bool> TryStartAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Start(leaseId, fencingToken, nowUtc), cancellationToken);

    public Task<bool> TrySaveCheckpointAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        string checkpoint, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId,
            task => task.SaveCheckpoint(leaseId, fencingToken, checkpoint, nowUtc), cancellationToken);

    public Task<bool> TryRenewLeaseAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId,
            task => task.RenewLease(leaseId, fencingToken, nowUtc, expiresAtUtc), cancellationToken);

    public Task<bool> TryCompleteAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Complete(leaseId, fencingToken, nowUtc), cancellationToken);

    public Task<bool> TryFailAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, string errorCode,\n        DateTimeOffset nowUtc, DateTimeOffset retryAtUtc, CancellationToken cancellationToken = default) =>\n        MutateAsync(tenantId, taskId, task => task.Fail(leaseId, fencingToken, errorCode, nowUtc, retryAtUtc), cancellationToken);\n\n    public Task<bool> TryHandoffAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
        string checkpoint, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId,
            task => task.Handoff(leaseId, fencingToken, checkpoint, nowUtc), cancellationToken);

    private async Task<bool> MutateAsync(Guid tenantId, Guid taskId, Action<WorkerTask> mutation,
        CancellationToken cancellationToken)
    {
        var task = await GetAsync(tenantId, taskId, cancellationToken);
        if (task is null)
            return false;
        try
        {
            mutation(task);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            db.Entry(task).State = EntityState.Detached;
            return false;
        }
        catch (DbUpdateConcurrencyException)
        {
            db.Entry(task).State = EntityState.Detached;
            return false;
        }
    }

    public void Add(WorkerTask task) => db.WorkerTasks.Add(task);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
