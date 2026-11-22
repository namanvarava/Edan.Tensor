using System.Data.Common;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

public sealed class TenantDatabaseConnectionFactory(ITenantDatabaseResolver resolver)
    : ITenantDatabaseConnectionFactory
{
    public async Task<DbConnection> OpenAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var resolution = await resolver.ResolveAsync(tenantId, cancellationToken);
        DbConnection connection = resolution.Provider switch
        {
            TenantDatabaseProvider.SQLite => new SqliteConnection(resolution.ConnectionString),
            TenantDatabaseProvider.PostgreSQL => new NpgsqlConnection(resolution.ConnectionString),
            TenantDatabaseProvider.SqlServer => new SqlConnection(resolution.ConnectionString),
            TenantDatabaseProvider.Oracle => new OracleConnection(resolution.ConnectionString),
            _ => throw new NotSupportedException($"지원하지 않는 Provider입니다: {resolution.Provider}")
        };

        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
