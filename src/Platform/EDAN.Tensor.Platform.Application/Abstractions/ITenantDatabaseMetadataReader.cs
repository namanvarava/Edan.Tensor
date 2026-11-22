using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Application.Abstractions;

public sealed record TenantDatabaseTableDescriptor(string SchemaName, string TableName, string? Comment);

public interface ITenantDatabaseMetadataReader
{
    Task<IReadOnlyList<TenantDatabaseTableDescriptor>> ListTablesAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}
