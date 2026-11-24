using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

public sealed record TenantDatabaseRoutineExecutionResult(int AffectedRows, IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows);

public interface ITenantDatabaseRoutineExecutor
{
    Task<TenantDatabaseRoutineExecutionResult> ExecuteAsync(TenantId tenantId, TenantDatabaseRoutineKind kind, string schemaName, string routineName, IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken = default);

    Task<TenantDatabaseRoutineExecutionResult> ExecuteAsync(TenantDatabaseBindingId bindingId, TenantId tenantId, TenantDatabaseRoutineKind kind, string schemaName, string routineName, IReadOnlyDictionary<string, string?> parameters, CancellationToken cancellationToken = default)
        => ExecuteAsync(tenantId, kind, schemaName, routineName, parameters, cancellationToken);
}
