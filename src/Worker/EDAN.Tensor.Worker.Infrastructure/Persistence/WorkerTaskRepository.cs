using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업을 Tenant 범위로 조회하고 추가합니다.</summary>
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

    public void Add(WorkerTask task) => db.WorkerTasks.Add(task);
}
