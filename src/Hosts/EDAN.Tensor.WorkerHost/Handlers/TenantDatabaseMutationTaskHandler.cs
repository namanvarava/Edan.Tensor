using System.Text.Json;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.WorkerHost.Handlers;

public sealed class TenantDatabaseMutationTaskHandler(IServiceScopeFactory scopeFactory) : IWorkerTaskHandler
{
    public const string TaskType = "tenant.database.mutation.v1";

    public Task<IWorkerTaskExecution> StartAsync(WorkerTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!task.PayloadReference.StartsWith("tenant-mutation:", StringComparison.Ordinal))
            throw new InvalidOperationException("지원하지 않는 테넌트 DB 변경 Payload Reference입니다.");
        var payload = task.PayloadReference["tenant-mutation:".Length..];
        var bytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
        var request = JsonSerializer.Deserialize<MutationPayload>(bytes) ?? throw new InvalidOperationException("테넌트 DB 변경 Payload가 비어 있습니다.");
        var steps = request.Steps.Select(x => new TenantDatabaseMutationStep(new TenantId(x.TenantId), x.Kind, x.SchemaName, x.RoutineName, x.Parameters ?? new Dictionary<string, string?>(), BindingId: x.BindingId is { } bindingId ? new TenantDatabaseBindingId(bindingId) : null)).ToArray();
        return Task.FromResult<IWorkerTaskExecution>(new MutationExecution(RunAsync(scopeFactory, task, steps, cancellationToken)));
    }

    private static async Task<TenantDatabaseMutationResult> RunAsync(IServiceScopeFactory scopeFactory, WorkerTask task, IReadOnlyList<TenantDatabaseMutationStep> steps, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<ITenantDatabaseMutationCoordinator>();
        return await coordinator.ExecuteAsync(new TenantId(task.TenantId), task.IdempotencyKey, steps, cancellationToken);
    }

    private sealed class MutationExecution(Task<TenantDatabaseMutationResult> run) : IWorkerTaskExecution
    {
        public async Task<WorkerTaskExecutionResult> CompletionResultAsync()
        {
            var result = await run;
            return result.Succeeded ? WorkerTaskExecutionResult.Success("tenant-mutation-result") : WorkerTaskExecutionResult.Failure("tenant.database.mutation_failed", DateTimeOffset.UtcNow.AddMinutes(1));
        }
        public Task<WorkerTaskExecutionResult> Completion => CompletionResultAsync();
        public async Task<string?> RequestCheckpointAndStopAsync(CancellationToken cancellationToken) => null;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed record MutationPayload(IReadOnlyList<MutationStep> Steps);
    private sealed record MutationStep(Guid TenantId, TenantDatabaseRoutineKind Kind, string SchemaName, string RoutineName, Dictionary<string, string?>? Parameters, Guid? BindingId = null);
}
