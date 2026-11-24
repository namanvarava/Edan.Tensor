using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

public sealed record TenantDatabaseConsistencyCheck(
    TenantId TenantId,
    TenantDatabaseRoutineKind Kind,
    string SchemaName,
    string RoutineName,
    IReadOnlyDictionary<string, string?> Parameters,
    TenantDatabaseBindingId? BindingId = null);

public sealed record TenantDatabaseConsistencyProviderResult(
    TenantId TenantId,
    bool Succeeded,
    string Fingerprint,
    int AffectedRows,
    IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows,
    string? Error,
    TenantDatabaseBindingId? BindingId = null);

public sealed record TenantDatabaseConsistencyResult(
    string Status,
    string Fingerprint,
    IReadOnlyList<TenantDatabaseConsistencyProviderResult> Providers);

public interface ITenantDatabaseConsistencyChecker
{
    Task<TenantDatabaseConsistencyResult> VerifyAsync(
        IReadOnlyList<TenantDatabaseConsistencyCheck> checks,
        CancellationToken cancellationToken = default);
}
