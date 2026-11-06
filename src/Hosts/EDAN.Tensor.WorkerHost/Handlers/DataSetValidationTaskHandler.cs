using EDAN.Tensor.DataSet.Application.UseCases;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.WorkerHost.Handlers;

/// <summary>불투명 실행 참조를 해석하여 DataSet 검증을 백그라운드에서 수행합니다.</summary>
public sealed class DataSetValidationTaskHandler(IServiceScopeFactory scopeFactory) : IWorkerTaskHandler
{
    /// <summary>검증 실행을 시작하고 Lease 인계에 사용할 실행 세션을 반환합니다.</summary>
    public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken)
    {
        if (!DataSetValidationTaskContract.TryParsePayloadReference(task.PayloadReference, out var executionId))
            throw new InvalidOperationException("지원하지 않는 DataSet 검증 Payload Reference입니다.");

        var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<DataSetValidationService>();
        var cancellation = new CancellationTokenSource();
        var run = service.RunAsync(task.TenantId, executionId, cancellation.Token);
        return Task.FromResult<IWorkerTaskExecution>(
            new DataSetValidationExecution(scope, cancellation, run, executionId));
    }

    private sealed class DataSetValidationExecution(
        AsyncServiceScope scope,
        CancellationTokenSource cancellation,
        Task<DataSetValidationRunOutcome> run,
        Guid executionId) : IWorkerTaskExecution
    {
        public Task<WorkerTaskExecutionResult> Completion => CompleteAsync();

        public async Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken)
        {
            cancellation.Cancel();
            var outcome = await run.WaitAsync(cancellationToken);
            return outcome == DataSetValidationRunOutcome.QueuedForRetry
                ? DataSetValidationTaskContract.CreatePayloadReference(executionId)
                : null;
        }

        public async ValueTask DisposeAsync()
        {
            cancellation.Cancel();
            try { await run.ConfigureAwait(false); }
            catch { }
            cancellation.Dispose();
            await scope.DisposeAsync();
        }

        private async Task<WorkerTaskExecutionResult> CompleteAsync()
        {
            try
            {
                var outcome = await run.ConfigureAwait(false);
                return outcome is DataSetValidationRunOutcome.Completed
                    or DataSetValidationRunOutcome.AlreadyCompleted
                    ? WorkerTaskExecutionResult.Success(
                        $"dataset-validation-result:{executionId:N}")
                    : WorkerTaskExecutionResult.Failure(
                        "dataset.validation.retry", DateTimeOffset.UtcNow.AddMinutes(1));
            }
            catch
            {
                return WorkerTaskExecutionResult.Failure(
                    "dataset.validation.execution_failed", DateTimeOffset.UtcNow.AddMinutes(1));
            }
        }
    }
}
