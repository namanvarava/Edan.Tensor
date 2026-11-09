using EDAN.Tensor.Worker.Application.ExternalDelivery;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.WorkerHost.Handlers;

/// <summary>불투명 전달 참조를 해석하여 외부 수신기로 전송합니다.</summary>
public sealed class ExternalDeliveryTaskHandler(IServiceScopeFactory scopeFactory) : IWorkerTaskHandler
{
    /// <summary>외부 전달 실행을 시작하고 응답 유실 시 Worker 재시도를 반환합니다.</summary>
    /// <param name="task">외부 전달의 Tenant와 불투명 전달 참조를 포함하는 Worker 작업입니다.</param>
    /// <param name="cancellationToken">조정기의 취소 신호이며 외부 HTTP 호출 중단과 연결됩니다.</param>
    /// <returns>전달 완료 결과와 안전한 종료 인계를 제공하는 실행 세션을 반환합니다.</returns>
    /// <exception cref="OperationCanceledException">외부 전달을 시작하기 전에 취소가 요청되면 예외를 반환합니다.</exception>
    public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ExternalDeliveryTaskContract.TryParsePayloadReference(task.PayloadReference, out var deliveryId))
            throw new InvalidOperationException("지원하지 않는 외부 전달 Payload Reference입니다.");

        var scope = scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ExternalDeliveryService>();
        // 조정기의 취소와 종료 인계 취소가 같은 외부 HTTP 호출을 중단하도록 연결합니다.
        var executionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var run = service.DeliverAsync(task.TenantId, deliveryId, executionCancellation.Token);
        return Task.FromResult<IWorkerTaskExecution>(
            new DeliveryExecution(scope, executionCancellation, run, deliveryId));
    }

    private sealed class DeliveryExecution(
        AsyncServiceScope scope,
        CancellationTokenSource executionCancellation,
        Task<ExternalDeliverySnapshot> run,
        Guid deliveryId) : IWorkerTaskExecution
    {
        public Task<WorkerTaskExecutionResult> Completion => CompleteAsync();

        public async Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken)
        {
            executionCancellation.Cancel();
            try { await run.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            return ExternalDeliveryTaskContract.CreatePayloadReference(deliveryId);
        }

        public async ValueTask DisposeAsync()
        {
            executionCancellation.Cancel();
            try { await run.ConfigureAwait(false); }
            catch { }
            executionCancellation.Dispose();
            await scope.DisposeAsync();
        }

        private async Task<WorkerTaskExecutionResult> CompleteAsync()
        {
            try
            {
                var result = await run.ConfigureAwait(false);
                return WorkerTaskExecutionResult.Success($"external-delivery-result:{result.Id:N}");
            }
            catch (ExternalDeliveryTransientException exception)
            {
                return WorkerTaskExecutionResult.Failure(exception.Code, DateTimeOffset.UtcNow.AddMinutes(1));
            }
            catch
            {
                return WorkerTaskExecutionResult.Failure(
                    "external.delivery.execution_failed", DateTimeOffset.UtcNow.AddMinutes(1));
            }
        }
    }
}
