using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

public sealed record TenantDatabaseMutationStep(TenantId TenantId, TenantDatabaseRoutineKind Kind, string SchemaName, string RoutineName, IReadOnlyDictionary<string, string?> Parameters, TenantDatabaseMutationStep? Compensation = null, TenantDatabaseBindingId? BindingId = null);
public sealed record TenantDatabaseMutationStepResult(int Index, bool Succeeded, bool Compensation, string? Error);
public sealed record TenantDatabaseMutationResult(bool Succeeded, bool Compensated, IReadOnlyList<TenantDatabaseMutationStepResult> Steps);

public interface ITenantDatabaseMutationCoordinator
{
    Task<TenantDatabaseMutationResult> ExecuteAsync(TenantId ownerTenantId, string idempotencyKey, IReadOnlyList<TenantDatabaseMutationStep> steps, CancellationToken cancellationToken = default);
}
