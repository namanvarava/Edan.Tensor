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
        return await OpenAsync(resolution, cancellationToken);
    }

    public async Task<DbConnection> OpenAsync(TenantDatabaseBindingId bindingId, TenantId tenantId, CancellationToken cancellationToken = default)
    {
        var resolution = await resolver.ResolveAsync(bindingId, cancellationToken);
        if (resolution.TenantId != tenantId)
            throw new InvalidOperationException("DB 바인딩이 요청 Tenant에 속하지 않습니다.");
        return await OpenAsync(resolution, cancellationToken);
    }

    private static async Task<DbConnection> OpenAsync(TenantDatabaseResolution resolution, CancellationToken cancellationToken)
    {
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
