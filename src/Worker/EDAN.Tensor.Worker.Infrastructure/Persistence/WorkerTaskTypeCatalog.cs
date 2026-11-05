using EDAN.Tensor.Worker.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.Worker.Infrastructure.Persistence;

/// <summary>Worker 작업 유형 카탈로그를 조회하고 Handler 구성과 동기화합니다.</summary>
public sealed class WorkerTaskTypeCatalog(WorkerDbContext db) : IWorkerTaskTypeCatalog
{
    public Task<bool> IsEnabledAsync(string taskType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(taskType);
        var normalized = taskType.Trim();
        return db.WorkerTaskTypes.AsNoTracking().AnyAsync(
            x => x.TaskType == normalized && x.IsEnabled, cancellationToken);
    }

    public async Task SynchronizeAsync(IReadOnlyCollection<WorkerTaskTypeDefinition> definitions,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        var normalized = definitions
            .Select(x => new WorkerTaskTypeDefinition(x.TaskType.Trim(), x.Description?.Trim()))
            .ToArray();
        if (normalized.Any(x => string.IsNullOrWhiteSpace(x.TaskType) || x.TaskType.Length > 120))
            throw new ArgumentException("작업 유형은 1~120자여야 합니다.", nameof(definitions));
        if (normalized.Select(x => x.TaskType).Distinct(StringComparer.Ordinal).Count() != normalized.Length)
            throw new InvalidOperationException("작업 유형 카탈로그 동기화 입력에 중복 유형이 있습니다.");

        foreach (var definition in normalized)
        {
            var entry = await db.WorkerTaskTypes.SingleOrDefaultAsync(
                x => x.TaskType == definition.TaskType, cancellationToken);
            if (entry is null)
                db.WorkerTaskTypes.Add(new WorkerTaskTypeCatalogEntry(
                    definition.TaskType, definition.Description, nowUtc));
            else
                entry.Synchronize(definition.Description, nowUtc);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
