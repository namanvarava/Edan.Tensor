using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업을 Tenant 범위로 조회하고 실행 상태를 동시성 검증과 함께 영속화합니다.</summary>
public sealed class WorkerTaskRepository(WorkerDbContext db) : IWorkerTaskRepository, IWorkerTaskDeadLetterRepository
{
    public Task<WorkerTask?> GetAsync(Guid tenantId, Guid taskId,
        CancellationToken cancellationToken = default) =>
        db.WorkerTasks.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == taskId, cancellationToken);

    /// <summary>실행 가능한 작업이 있는 Tenant를 준비 시각이 오래된 순서로 조회합니다.</summary>
    public async Task<IReadOnlyList<Guid>> GetReadyTenantIdsAsync(
        DateTimeOffset nowUtc, int take, CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(take), "Tenant 조회 건수는 1~1000이어야 합니다.");

        // SQLite DateTimeOffset 비교 제약을 피하기 위해 시각 비교는 메모리에서 수행합니다.
        var candidates = await db.WorkerTasks.AsNoTracking()
            .Where(x => x.Status == WorkerTaskStatus.Pending ||
                        x.Status == WorkerTaskStatus.RetryScheduled ||
                        x.Status == WorkerTaskStatus.Checkpointed)
            .Select(x => new { x.TenantId, x.NextAttemptAtUtc, x.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        return candidates
            .Where(x => !x.NextAttemptAtUtc.HasValue ||
                        x.NextAttemptAtUtc.Value <= nowUtc.ToUniversalTime())
            .GroupBy(x => x.TenantId)
            .Select(group => new
            {
                TenantId = group.Key,
                ReadyAtUtc = group.Min(x => x.NextAttemptAtUtc ?? x.CreatedAtUtc)
            })
            .OrderBy(x => x.ReadyAtUtc)
            .ThenBy(x => x.TenantId)
            .Take(take)
            .Select(x => x.TenantId)
            .ToArray();
    }

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

    public Task<bool> TryFailAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, string errorCode,
        DateTimeOffset nowUtc, DateTimeOffset retryAtUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Fail(leaseId, fencingToken, errorCode, nowUtc, retryAtUtc), cancellationToken);

    public Task<bool> TryHandoffAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
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

    /// <summary>특정 Tenant의 DLQ 작업을 최신 실패 순서로 조회합니다.</summary>
    public async Task<IReadOnlyList<WorkerTask>> GetDeadLetteredAsync(Guid tenantId, int skip, int take,
        CancellationToken cancellationToken = default)
    {
        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip));
        if (take is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(take));
        return await db.WorkerTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == WorkerTaskStatus.DeadLettered)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>원본 DLQ 항목을 보존하면서 새 작업과 재처리 이력을 원자적으로 생성합니다.</summary>
    public async Task<Guid?> ReplayAsync(Guid tenantId, Guid taskId, string requestedBy,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var original = await db.WorkerTasks.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == taskId, cancellationToken);
        if (original is null || original.Status != WorkerTaskStatus.DeadLettered)
            return null;

        var replayTask = WorkerTask.Create(tenantId, original.TaskType,
            "replay:" + Guid.NewGuid().ToString("N"), original.PayloadReference, nowUtc, original.MaxAttempts);
        db.WorkerTasks.Add(replayTask);
        db.WorkerTaskReplayRecords.Add(new WorkerTaskReplayRecord(
            tenantId, original.Id, replayTask.Id, requestedBy, nowUtc));
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return replayTask.Id;
    }

    public void Add(WorkerTask task) => db.WorkerTasks.Add(task);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
