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
            TenantDatabaseProvider.Oracle => "SELECT OWNER, TABLE_NAME, COMMENTS FROM ALL_TAB_COMMENTS WHERE OWNER = UPPER(:schema) ORDER BY TABLE_NAME",
            _ => throw new NotSupportedException($"지원하지 않는 Provider입니다: {resolution.Provider}")
        };
        var result = new List<TenantDatabaseTableDescriptor>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new(reader.GetString(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetValue(2)?.ToString()));
        return result;
    }

    public async Task<IReadOnlyList<TenantDatabaseColumnDescriptor>> ListColumnsAsync(TenantId tenantId, string schemaName, string tableName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) || string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("스키마와 테이블 이름이 필요합니다.");
        var resolution = await resolver.ResolveAsync(tenantId, cancellationToken);
        await using var connection = await new TenantDatabaseConnectionFactory(new StaticResolver(resolution)).OpenAsync(tenantId, cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = resolution.Provider switch
        {
            TenantDatabaseProvider.PostgreSQL => "SELECT table_schema, table_name, column_name, data_type, is_nullable, ordinal_position, NULL FROM information_schema.columns WHERE table_schema = @schema AND table_name = @table ORDER BY ordinal_position",
            TenantDatabaseProvider.SQLite => "SELECT @schema, @table, name, type, CASE WHEN notnull=1 THEN 'NO' ELSE 'YES' END, cid + 1, NULL FROM pragma_table_info(@table) ORDER BY cid",
            TenantDatabaseProvider.SqlServer => "SELECT s.name, t.name, c.name, ty.name, CASE WHEN c.is_nullable=1 THEN 'YES' ELSE 'NO' END, c.column_id, CAST(ep.value AS nvarchar(max)) FROM sys.columns c JOIN sys.tables t ON t.object_id=c.object_id JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id LEFT JOIN sys.extended_properties ep ON ep.major_id=c.object_id AND ep.minor_id=c.column_id AND ep.name='MS_Description' WHERE s.name=@schema AND t.name=@table ORDER BY c.column_id",
            TenantDatabaseProvider.Oracle => "SELECT OWNER, TABLE_NAME, COLUMN_NAME, DATA_TYPE, NULLABLE, COLUMN_ID, COMMENTS FROM ALL_TAB_COLUMNS c LEFT JOIN ALL_COL_COMMENTS cc ON cc.OWNER=c.OWNER AND cc.TABLE_NAME=c.TABLE_NAME AND cc.COLUMN_NAME=c.COLUMN_NAME WHERE c.OWNER=UPPER(:schema) AND c.TABLE_NAME=UPPER(:table) ORDER BY c.COLUMN_ID",
            _ => throw new NotSupportedException($"지원하지 않는 Provider입니다: {resolution.Provider}")
        };
        AddParameter(command, "schema", schemaName);
        AddParameter(command, "table", tableName);
        var result = new List<TenantDatabaseColumnDescriptor>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), string.Equals(reader.GetValue(4)?.ToString(), "YES", StringComparison.OrdinalIgnoreCase), reader.IsDBNull(5) ? null : Convert.ToInt32(reader.GetValue(5)), reader.IsDBNull(6) ? null : reader.GetValue(6)?.ToString()));
        return result;
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, string value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@" + name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed class StaticResolver(TenantDatabaseResolution value) : ITenantDatabaseResolver
    {
        public Task<TenantDatabaseResolution> ResolveAsync(TenantId _, CancellationToken __ = default) => Task.FromResult(value);
    }
}
