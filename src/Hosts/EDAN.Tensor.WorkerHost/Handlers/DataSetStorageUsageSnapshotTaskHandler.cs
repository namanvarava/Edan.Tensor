using EDAN.Tensor.HostServices.Services;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.WorkerHost.Handlers;

/// <summary>WorkerTask 큐에서 Tenant별 DataSet 사용량 스냅샷을 실행합니다.</summary>
public sealed class DataSetStorageUsageSnapshotTaskHandler(IServiceScopeFactory scopeFactory) : IWorkerTaskHandler
{
    public async Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken)
    {
        if (!string.Equals(task.PayloadReference,
                BackgroundServices.DataSetStorageUsageSnapshotWorker.PayloadReference, StringComparison.Ordinal))
            throw new InvalidOperationException("지원하지 않는 DataSet 스냅샷 Payload Reference입니다.");

        await using var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<DataSetStorageUsageSnapshotService>();
        await service.CaptureTenantAsync(task.TenantId, cancellationToken);
        return new CompletedWorkerTaskExecution();
    }

    private sealed class CompletedWorkerTaskExecution : IWorkerTaskExecution
    {
        public Task<WorkerTaskExecutionResult> Completion { get; } =
            Task.FromResult(WorkerTaskExecutionResult.Success());
        public Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken)
            => Task.FromResult<string?>(null);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
