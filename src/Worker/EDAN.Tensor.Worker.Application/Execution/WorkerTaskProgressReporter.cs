using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Execution;

/// <summary>실행 중인 Handler가 현재 Lease 소유권으로 진행 상태를 안전하게 보고합니다.</summary>
public sealed class WorkerTaskProgressReporter(IWorkerTaskRepository repository, TimeProvider timeProvider)
{
    public Task<bool> ReportAsync(WorkerTask task, int percentage, string stage, string? message = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (!task.LeaseId.HasValue)
            return Task.FromResult(false);
        return repository.TryReportProgressAsync(task.TenantId, task.Id, task.LeaseId.Value,
            task.FencingToken, percentage, stage, message, timeProvider.GetUtcNow(), cancellationToken);
    }
}
