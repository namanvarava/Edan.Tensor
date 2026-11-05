using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Domain.Model;
using Microsoft.Data.Sqlite;

namespace EDAN.Tensor.DataSet.Infrastructure.Storage;

/// <summary>DataSet 콘텐츠를 전용 SQLite DB에 저장하며 메타데이터 DB와 물리적으로 분리합니다.</summary>
public sealed class SqliteDataSetStorageAdapter : IDataSetStorageAdapter, IDataSetStorageUsageReader
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

    public async Task WriteAsync(DataSetStorageAllocation allocation, StorageReference reference, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
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
        await using var command = connection.CreateCommand();
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
            SELECT COALESCE(SUM(length(Content)), 0),
                   COUNT(DISTINCT DataSetId),
                   COALESCE(SUM(CASE WHEN DataSetId = $dataSetId THEN length(Content) ELSE 0 END), 0)
            FROM DataSetContent
            WHERE TenantId = $tenantId AND OrganizationId = $organizationId AND ProjectId = $projectId;
            """;
        command.Parameters.AddWithValue("$dataSetId", allocation.DataSetId.Value.ToString("D"));
        command.Parameters.AddWithValue("$tenantId", allocation.Scope.TenantId.ToString("D"));
        command.Parameters.AddWithValue("$organizationId", allocation.Scope.OrganizationId.ToString("D"));
        command.Parameters.AddWithValue("$projectId", allocation.Scope.ProjectId.ToString("D"));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new DataSetStorageUsage(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2));
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
