using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

public sealed class WorkerTaskRepository(WorkerDbContext db) : IWorkerTaskRepository, IWorkerTaskDeadLetterRepository
{
    public Task<WorkerTask?> GetAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default) =>
        db.WorkerTasks.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == taskId, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetReadyTenantIdsAsync(DateTimeOffset nowUtc, int take, CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(take));
        var nowMilliseconds = nowUtc.ToUnixTimeMilliseconds();
        return await db.WorkerTasks.AsNoTracking()
            .Where(x => x.Status == WorkerTaskStatus.Pending || x.Status == WorkerTaskStatus.RetryScheduled || x.Status == WorkerTaskStatus.Checkpointed)
            .Where(x => EF.Property<long>(x, "ReadyAtUtcUnixMilliseconds") <= nowMilliseconds)
            .GroupBy(x => x.TenantId)
            .Select(group => new
            {
                TenantId = group.Key,
                ReadyAtMilliseconds = group.Min(x => EF.Property<long>(x, "ReadyAtUtcUnixMilliseconds"))
            })
            .OrderBy(x => x.ReadyAtMilliseconds)
            .ThenBy(x => x.TenantId)
            .Take(take)
            .Select(x => x.TenantId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkerTask>> GetReadyAsync(Guid tenantId, DateTimeOffset nowUtc, int take, CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(take));
        var nowMilliseconds = nowUtc.ToUnixTimeMilliseconds();
        return await db.WorkerTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && (x.Status == WorkerTaskStatus.Pending || x.Status == WorkerTaskStatus.RetryScheduled || x.Status == WorkerTaskStatus.Checkpointed))
            .Where(x => EF.Property<long>(x, "ReadyAtUtcUnixMilliseconds") <= nowMilliseconds)
            .OrderBy(x => EF.Property<long>(x, "ReadyAtUtcUnixMilliseconds"))
            .ThenBy(x => x.Id)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    /// <summary>만료된 Lease를 재시도 대기 또는 DLQ로 전이하여 Worker 비정상 종료 후 작업을 복구합니다.</summary>
    public async Task<int> RecoverExpiredLeasesAsync(DateTimeOffset nowUtc, int take, CancellationToken cancellationToken = default)
    {
        if (take is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(take));
        var nowMilliseconds = nowUtc.ToUnixTimeMilliseconds();
        var candidates = await db.WorkerTasks
            .Where(x => x.Status == WorkerTaskStatus.Leased || x.Status == WorkerTaskStatus.Running || x.Status == WorkerTaskStatus.Checkpointed)
            .Where(x => EF.Property<long?>(x, "LeaseExpiresAtUtcUnixMilliseconds").HasValue
                && EF.Property<long?>(x, "LeaseExpiresAtUtcUnixMilliseconds") <= nowMilliseconds)
            .OrderBy(x => EF.Property<long?>(x, "LeaseExpiresAtUtcUnixMilliseconds"))
            .ThenBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
        var recovered = 0;
        foreach (var task in candidates)
        {
            if (!task.RecoverExpiredLease(nowUtc)) continue;
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                recovered++;
            }
            catch (DbUpdateConcurrencyException)
            {
                db.Entry(task).State = EntityState.Detached;
            }
        }
        return recovered;
    }

    public async Task<WorkerTask?> TryAcquireLeaseAsync(Guid tenantId, Guid taskId, string workerId, Guid leaseId,
        DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default)
    {
        var task = await GetAsync(tenantId, taskId, cancellationToken);
        if (task is null) return null;
        try { task.AcquireLease(workerId, leaseId, nowUtc, expiresAtUtc); await db.SaveChangesAsync(cancellationToken); return task; }
        catch (InvalidOperationException) { db.Entry(task).State = EntityState.Detached; return null; }
        catch (DbUpdateConcurrencyException) { db.Entry(task).State = EntityState.Detached; return null; }
    }

    public Task<bool> TryStartAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Start(leaseId, fencingToken, nowUtc), cancellationToken);
    public Task<bool> TrySaveCheckpointAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, string checkpoint, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.SaveCheckpoint(leaseId, fencingToken, checkpoint, nowUtc), cancellationToken);
    public Task<bool> TryRenewLeaseAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.RenewLease(leaseId, fencingToken, nowUtc, expiresAtUtc), cancellationToken);
    public Task<bool> TryReportProgressAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, int percentage, string stage, string? message, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.ReportProgress(leaseId, fencingToken, percentage, stage, message, nowUtc), cancellationToken);
    public Task<bool> TryCompleteAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, DateTimeOffset nowUtc, string? resultReference = null, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Complete(leaseId, fencingToken, nowUtc, resultReference), cancellationToken);
    public Task<bool> TryFailAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, string errorCode, DateTimeOffset nowUtc, DateTimeOffset retryAtUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Fail(leaseId, fencingToken, errorCode, nowUtc, retryAtUtc), cancellationToken);
    public Task<bool> TryHandoffAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken, string checkpoint, DateTimeOffset nowUtc, CancellationToken cancellationToken = default) =>
        MutateAsync(tenantId, taskId, task => task.Handoff(leaseId, fencingToken, checkpoint, nowUtc), cancellationToken);

    private async Task<bool> MutateAsync(Guid tenantId, Guid taskId, Action<WorkerTask> mutation, CancellationToken cancellationToken)
    {
        var task = await GetAsync(tenantId, taskId, cancellationToken);
        if (task is null) return false;
        try { mutation(task); await db.SaveChangesAsync(cancellationToken); return true; }
        catch (InvalidOperationException) { db.Entry(task).State = EntityState.Detached; return false; }
        catch (DbUpdateConcurrencyException) { db.Entry(task).State = EntityState.Detached; return false; }
    }

    public async Task<IReadOnlyList<WorkerTask>> GetDeadLetteredAsync(Guid tenantId, int skip, int take, CancellationToken cancellationToken = default)
    {
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        if (take is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(take));
        return await db.WorkerTasks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == WorkerTaskStatus.DeadLettered)
            .OrderByDescending(x => EF.Property<long>(x, "CreatedAtUtcUnixMilliseconds"))
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Guid?> ReplayAsync(Guid tenantId, Guid taskId, Guid replayRequestId, string requestedBy,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var prior = await db.WorkerTaskReplayRecords.AsNoTracking().SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.OriginalTaskId == taskId && x.ReplayRequestId == replayRequestId,
                cancellationToken);
            if (prior is not null) return prior.ReplayTaskId;

            var original = await db.WorkerTasks.SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == taskId, cancellationToken);
            if (original is null || original.Status != WorkerTaskStatus.DeadLettered) return null;
            var replayTask = WorkerTask.Create(tenantId, original.TaskType, "replay:" + Guid.NewGuid().ToString("N"),
                original.PayloadReference, nowUtc, original.MaxAttempts);
            db.WorkerTasks.Add(replayTask);
            db.WorkerTaskReplayRecords.Add(new WorkerTaskReplayRecord(
                tenantId, original.Id, replayTask.Id, replayRequestId, requestedBy, nowUtc));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return replayTask.Id;
        }
        catch (DbUpdateException)
        {
            foreach (var entry in db.ChangeTracker.Entries()
                         .Where(x => x.State == EntityState.Added
                             && (x.Entity is WorkerTask || x.Entity is WorkerTaskReplayRecord))
                         .ToArray())
                entry.State = EntityState.Detached;

            var prior = await db.WorkerTaskReplayRecords.AsNoTracking().SingleOrDefaultAsync(
                x => x.TenantId == tenantId && x.OriginalTaskId == taskId && x.ReplayRequestId == replayRequestId,
                cancellationToken);
            if (prior is not null) return prior.ReplayTaskId;
            throw;
        }
    }

    public async Task<WorkerTaskEnqueueResult> EnqueueAsync(WorkerTask task, CancellationToken cancellationToken = default)
    {
        var existing = await FindByIdempotencyKeyAsync(task.TenantId, task.IdempotencyKey, cancellationToken);
        if (existing is not null) return CreateEnqueueResult(existing, task, created: false);
        db.WorkerTasks.Add(task);
        try { await db.SaveChangesAsync(cancellationToken); return new WorkerTaskEnqueueResult(task, Created: true, Conflict: false); }
        catch (DbUpdateException)
        {
            db.Entry(task).State = EntityState.Detached;
            existing = await FindByIdempotencyKeyAsync(task.TenantId, task.IdempotencyKey, cancellationToken);
            if (existing is null) throw;
            return CreateEnqueueResult(existing, task, created: false);
        }
    }

    private Task<WorkerTask?> FindByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey, CancellationToken cancellationToken) =>
        db.WorkerTasks.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
    private static WorkerTaskEnqueueResult CreateEnqueueResult(WorkerTask existing, WorkerTask requested, bool created) =>
        new(existing, created, existing.TaskType != requested.TaskType || existing.PayloadReference != requested.PayloadReference || existing.MaxAttempts != requested.MaxAttempts);
    public void Add(WorkerTask task) => db.WorkerTasks.Add(task);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
