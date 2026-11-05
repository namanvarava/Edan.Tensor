using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Domain.Model;
using Microsoft.Data.Sqlite;

namespace EDAN.Tensor.DataSet.Infrastructure.Storage;

/// <summary>DataSet 콘텐츠를 전용 SQLite DB에 저장하며 메타데이터 DB와 물리적으로 분리합니다.</summary>
public sealed class SqliteDataSetStorageAdapter : IDataSetStorageAdapter, IDataSetStorageUsageReader, IDataSetTenantStorageUsageReader
{
    private readonly string connectionString;
    private readonly long maxContentBytes;

    public SqliteDataSetStorageAdapter(string connectionString, long maxContentBytes = 67_108_864)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        if (maxContentBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxContentBytes));
        this.connectionString = connectionString;
        this.maxContentBytes = maxContentBytes;
    }
    public string AdapterId => "sqlite";

    public async Task<StorageReference> AllocateAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        var reference = new StorageReference("sqlite:" + Guid.NewGuid().ToString("N"));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO DataSetContent (StorageReference, DataSetId, TenantId, OrganizationId, ProjectId, Content)
            VALUES ($reference, $dataSetId, $tenantId, $organizationId, $projectId, X'');
            """;
        AddScope(command, allocation, reference);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return reference;
    }

    public async Task WriteAsync(DataSetStorageAllocation allocation, StorageReference reference, Stream content, DataSetStorageQuotaPolicy quotaPolicy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(allocation);
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(quotaPolicy);
        if (quotaPolicy.WarningThresholdPercent is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(quotaPolicy), "Warning threshold must be between 1 and 100.");
        await using var buffer = new MemoryStream();
        var chunk = new byte[81_920];
        long totalBytes = 0;
        int bytesRead;
        while ((bytesRead = await content.ReadAsync(chunk.AsMemory(), cancellationToken)) > 0)
        {
            totalBytes += bytesRead;
            if (totalBytes > maxContentBytes)
                throw new InvalidDataException("DataSet 콘텐츠 크기 제한을 초과했습니다.");
            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
        }
        await using var connection = await OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction(deferred: false);
        await using (var usageCommand = connection.CreateCommand())
        {
            usageCommand.Transaction = transaction;
            usageCommand.CommandText = """
                SELECT
                    COALESCE(SUM(CASE WHEN TenantId = $tenantId THEN length(Content) ELSE 0 END), 0),
                    COALESCE(SUM(CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId THEN length(Content) ELSE 0 END), 0),
                    COALESCE(SUM(CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId THEN length(Content) ELSE 0 END), 0),
                    COALESCE(SUM(CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId AND DataSetId = $dataSetId THEN length(Content) ELSE 0 END), 0)
                FROM DataSetContent
                WHERE StorageReference <> $reference;
                """;
            usageCommand.Parameters.AddWithValue("$reference", reference.Value);
            usageCommand.Parameters.AddWithValue("$dataSetId", allocation.DataSetId.Value.ToString("D"));
            usageCommand.Parameters.AddWithValue("$tenantId", allocation.Scope.TenantId.ToString("D"));
            usageCommand.Parameters.AddWithValue("$organizationId", allocation.Scope.OrganizationId.ToString("D"));
            usageCommand.Parameters.AddWithValue("$projectId", allocation.Scope.ProjectId.ToString("D"));
            await using var reader = await usageCommand.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var projected = new[] { reader.GetInt64(0) + totalBytes, reader.GetInt64(1) + totalBytes, reader.GetInt64(2) + totalBytes, reader.GetInt64(3) + totalBytes };
            EnsureWithinLimit("Tenant", quotaPolicy.TenantLimitBytes, projected[0]);
            EnsureWithinLimit("Organization", quotaPolicy.OrganizationLimitBytes, projected[1]);
            EnsureWithinLimit("Project", quotaPolicy.ProjectLimitBytes, projected[2]);
            EnsureWithinLimit("DataSet", quotaPolicy.DataSetLimitBytes, projected[3]);
        }
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                UPDATE DataSetContent SET Content = $content
                WHERE StorageReference = $reference AND DataSetId = $dataSetId
                  AND TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId;
                """;
            AddScope(command, allocation, reference);
            command.Parameters.Add("$content", SqliteType.Blob).Value = buffer.ToArray();
            if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
                throw new KeyNotFoundException("Tenant 소유 범위에서 저장소 참조를 찾을 수 없습니다.");
        }
        transaction.Commit();
    }

    public async Task<Stream?> OpenReadAsync(DataSetStorageAllocation allocation, StorageReference reference, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Content FROM DataSetContent
            WHERE StorageReference = $reference AND DataSetId = $dataSetId
              AND TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId;
            """;
        AddScope(command, allocation, reference);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is byte[] bytes ? new MemoryStream(bytes, writable: false) : null;
    }

    public async Task<DataSetStorageUsage> GetUsageAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                COALESCE(SUM(CASE WHEN TenantId = $tenantId THEN length(Content) ELSE 0 END), 0),
                COUNT(DISTINCT CASE WHEN TenantId = $tenantId THEN DataSetId END),
                COALESCE(SUM(CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId THEN length(Content) ELSE 0 END), 0),
                COUNT(DISTINCT CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId THEN DataSetId END),
                COALESCE(SUM(CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId THEN length(Content) ELSE 0 END), 0),
                COUNT(DISTINCT CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId THEN DataSetId END),
                COALESCE(SUM(CASE WHEN TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId AND DataSetId = $dataSetId THEN length(Content) ELSE 0 END), 0)
            FROM DataSetContent;
            """;
        command.Parameters.AddWithValue("$dataSetId", allocation.DataSetId.Value.ToString("D"));
        command.Parameters.AddWithValue("$tenantId", allocation.Scope.TenantId.ToString("D"));
        command.Parameters.AddWithValue("$organizationId", allocation.Scope.OrganizationId.ToString("D"));
        command.Parameters.AddWithValue("$projectId", allocation.Scope.ProjectId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new DataSetStorageUsage(
            reader.GetInt64(0), reader.GetInt64(1),
            reader.GetInt64(2), reader.GetInt64(3),
            reader.GetInt64(4), reader.GetInt64(5),
            reader.GetInt64(6));
    }

    public async Task<DataSetTenantStorageUsage> GetTenantUsageAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                COALESCE(SUM(length(Content)), 0),
                COUNT(DISTINCT DataSetId)
            FROM DataSetContent
            WHERE TenantId = $tenantId;
            """;
        command.Parameters.AddWithValue("$tenantId", tenantId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new DataSetTenantStorageUsage(reader.GetInt64(0), reader.GetInt64(1));
    }

    public async Task ReleaseAsync(DataSetStorageAllocation allocation, StorageReference reference, CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM DataSetContent
            WHERE StorageReference = $reference AND DataSetId = $dataSetId
              AND TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId;
            """;
        AddScope(command, allocation, reference);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void EnsureWithinLimit(string scope, long? limitBytes, long projectedBytes)
    {
        if (limitBytes is < 0) throw new ArgumentOutOfRangeException(nameof(limitBytes));
        if (limitBytes is long limit && projectedBytes > limit)
            throw new DataSetStorageQuotaExceededException(scope, limit, projectedBytes);
    }

    private async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS DataSetContent (
                StorageReference TEXT NOT NULL PRIMARY KEY,
                DataSetId TEXT NOT NULL,
                TenantId TEXT NOT NULL,
                OrganizationId TEXT NOT NULL,
                ProjectId TEXT NOT NULL,
                Content BLOB NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_DataSetContent_Owner
                ON DataSetContent (TenantId, OrganizationId, ProjectId, DataSetId);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }

    private static void AddScope(SqliteCommand command, DataSetStorageAllocation allocation, StorageReference reference)
    {
        ArgumentNullException.ThrowIfNull(allocation);
        ArgumentNullException.ThrowIfNull(reference);
        command.Parameters.AddWithValue("$reference", reference.Value);
        command.Parameters.AddWithValue("$dataSetId", allocation.DataSetId.Value.ToString("D"));
        command.Parameters.AddWithValue("$tenantId", allocation.Scope.TenantId.ToString("D"));
        command.Parameters.AddWithValue("$organizationId", allocation.Scope.OrganizationId.ToString("D"));
        command.Parameters.AddWithValue("$projectId", allocation.Scope.ProjectId.ToString("D"));
    }
}

/// <summary>단일 SQLite 어댑터를 식별자로 해석합니다.</summary>
public sealed class DataSetStorageAdapterResolver(IDataSetStorageAdapter adapter) : IDataSetStorageAdapterResolver
{
    public IDataSetStorageAdapter? Resolve(string adapterId) =>
        string.Equals(adapter.AdapterId, adapterId, StringComparison.OrdinalIgnoreCase) ? adapter : null;
}
