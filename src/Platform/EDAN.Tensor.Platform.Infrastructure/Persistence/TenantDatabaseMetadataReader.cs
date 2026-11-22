using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseMetadataReader(ITenantDatabaseResolver resolver) : ITenantDatabaseMetadataReader
{
    public async Task<IReadOnlyList<TenantDatabaseTableDescriptor>> ListTablesAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var resolution = await resolver.ResolveAsync(tenantId, cancellationToken);
        await using var connection = await new TenantDatabaseConnectionFactory(new StaticResolver(resolution)).OpenAsync(tenantId, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = resolution.Provider switch
        {
            TenantDatabaseProvider.PostgreSQL => "SELECT table_schema, table_name, obj_description((quote_ident(table_schema)||'.'||quote_ident(table_name))::regclass) FROM information_schema.tables WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog','information_schema') ORDER BY table_schema, table_name",
            TenantDatabaseProvider.SqlServer => "SELECT s.name, t.name, CAST(ep.value AS nvarchar(max)) FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id LEFT JOIN sys.extended_properties ep ON ep.major_id=t.object_id AND ep.minor_id=0 AND ep.name='MS_Description' ORDER BY s.name, t.name",
            TenantDatabaseProvider.SQLite => "SELECT 'main', name, NULL FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name",
            TenantDatabaseProvider.Oracle => throw new NotSupportedException("Oracle 메타데이터 어댑터는 Oracle Provider 단계에서 활성화해야 합니다."),
            _ => throw new NotSupportedException($"지원하지 않는 Provider입니다: {resolution.Provider}")
        };
        var result = new List<TenantDatabaseTableDescriptor>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetValue(2)?.ToString()));
        return result;
    }

    private sealed class StaticResolver(TenantDatabaseResolution value) : ITenantDatabaseResolver
    {
        public Task<TenantDatabaseResolution> ResolveAsync(TenantId _, CancellationToken __ = default) => Task.FromResult(value);
    }
}
