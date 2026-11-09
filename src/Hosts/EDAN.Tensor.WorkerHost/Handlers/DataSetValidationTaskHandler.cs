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
    /// <param name="task">DataSet 검증의 Tenant, Fencing Token 및 불투명 실행 참조를 포함한 작업입니다.</param>
    /// <param name="cancellationToken">조정기가 요청한 취소 신호이며 검증 실행의 취소 경계와 연결됩니다.</param>
    /// <returns>검증 완료 결과와 안전한 종료 인계를 제공하는 실행 세션을 반환합니다.</returns>
    /// <exception cref="OperationCanceledException">검증 실행을 시작하기 전에 취소가 요청되면 예외를 반환합니다.</exception>
    public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!DataSetValidationTaskContract.TryParsePayloadReference(task.PayloadReference, out var executionId))
            throw new InvalidOperationException("지원하지 않는 DataSet 검증 Payload Reference입니다.");

        var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<DataSetValidationService>();
        // 조정기의 취소와 종료 인계의 명시적 취소가 같은 검증 실행을 중단하도록 연결합니다.
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var run = service.RunAsync(task.TenantId, executionId, task.FencingToken, cancellation.Token);
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
