using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

public sealed record TenantDatabaseTableDescriptor(string SchemaName, string TableName, string? Comment);
public sealed record TenantDatabaseColumnDescriptor(string SchemaName, string TableName, string ColumnName, string DataType, bool IsNullable, int? OrdinalPosition, string? Comment);
public sealed record TenantDatabaseRoutineDescriptor(string SchemaName, string RoutineName, string RoutineKind, string? Comment);

public interface ITenantDatabaseMetadataReader
{
    Task<IReadOnlyList<TenantDatabaseTableDescriptor>> ListTablesAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantDatabaseColumnDescriptor>> ListColumnsAsync(TenantId tenantId, string schemaName, string tableName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantDatabaseRoutineDescriptor>> ListRoutinesAsync(TenantId tenantId, string schemaName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantDatabaseTableDescriptor>> ListTablesAsync(TenantDatabaseBindingId bindingId, TenantId tenantId, CancellationToken cancellationToken = default)
        => ListTablesAsync(tenantId, cancellationToken);
    Task<IReadOnlyList<TenantDatabaseColumnDescriptor>> ListColumnsAsync(TenantDatabaseBindingId bindingId, TenantId tenantId, string schemaName, string tableName, CancellationToken cancellationToken = default)
        => ListColumnsAsync(tenantId, schemaName, tableName, cancellationToken);
    Task<IReadOnlyList<TenantDatabaseRoutineDescriptor>> ListRoutinesAsync(TenantDatabaseBindingId bindingId, TenantId tenantId, string schemaName, CancellationToken cancellationToken = default)
        => ListRoutinesAsync(tenantId, schemaName, cancellationToken);
}
