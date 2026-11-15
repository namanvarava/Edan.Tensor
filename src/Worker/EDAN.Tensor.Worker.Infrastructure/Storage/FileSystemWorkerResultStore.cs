using System.Security.Cryptography;
using System.Text.Json;
using EDAN.Tensor.Worker.Application.Abstractions;

namespace EDAN.Tensor.Worker.Infrastructure.Storage;

/// <summary>개발·단일 노드 배포에서 사용할 Tenant/Task 격리 파일 저장소입니다.</summary>
public sealed class FileSystemWorkerResultStore(string rootPath) : IWorkerResultStore
{
    private readonly string root = Path.GetFullPath(rootPath);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<WorkerResultMetadata> SaveAsync(Guid tenantId, Guid taskId, string contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty || taskId == Guid.Empty) throw new ArgumentException("Tenant와 Task 식별자가 필요합니다.");
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(content);
        var normalizedType = contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
        if (normalizedType.Length > 200) throw new ArgumentException("Content-Type이 너무 깁니다.", nameof(contentType));
        var resultId = Guid.NewGuid().ToString("N");
        var directory = GetTaskDirectory(tenantId, taskId);
        Directory.CreateDirectory(directory);
        var dataPath = Path.Combine(directory, resultId + ".bin");
        var metadataPath = Path.Combine(directory, resultId + ".json");
        var temporaryPath = dataPath + ".tmp";
        long length = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
        {
            var buffer = new byte[64 * 1024];
            int read;
            while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                hash.AppendData(buffer, 0, read);
                length += read;
            }
        }
        File.Move(temporaryPath, dataPath);
        var reference = $"worker-file:{tenantId:D}/{taskId:D}/{resultId}";
        var metadata = new WorkerResultMetadata(tenantId, taskId, reference, normalizedType, length,
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(), DateTimeOffset.UtcNow);
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, JsonOptions), cancellationToken);
        return metadata;
    }

    public async Task<WorkerResultMetadata?> GetMetadataAsync(Guid tenantId, Guid taskId, string resultReference,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetResultId(tenantId, taskId, resultReference, out var resultId)) return null;
        var metadataPath = Path.Combine(GetTaskDirectory(tenantId, taskId), resultId + ".json");
        if (!File.Exists(metadataPath)) return null;
        await using var input = File.OpenRead(metadataPath);
        var metadata = await JsonSerializer.DeserializeAsync<WorkerResultMetadata>(input, JsonOptions, cancellationToken);
        return metadata is not null && metadata.TenantId == tenantId && metadata.TaskId == taskId &&
            string.Equals(metadata.ResultReference, resultReference, StringComparison.OrdinalIgnoreCase)
            ? metadata
            : null;
    }

    public Task<Stream?> OpenReadAsync(WorkerResultMetadata metadata, CancellationToken cancellationToken = default)
    {
        if (!TryGetResultId(metadata.TenantId, metadata.TaskId, metadata.ResultReference, out var resultId))
            return Task.FromResult<Stream?>(null);
        var path = Path.Combine(GetTaskDirectory(metadata.TenantId, metadata.TaskId), resultId + ".bin");
        Stream? stream = File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, useAsync: true) : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(WorkerResultMetadata metadata, CancellationToken cancellationToken = default)
    {
        if (TryGetResultId(metadata.TenantId, metadata.TaskId, metadata.ResultReference, out var resultId))
        {
            File.Delete(Path.Combine(GetTaskDirectory(metadata.TenantId, metadata.TaskId), resultId + ".bin"));
            File.Delete(Path.Combine(GetTaskDirectory(metadata.TenantId, metadata.TaskId), resultId + ".json"));
        }
        return Task.CompletedTask;
    }

    private string GetTaskDirectory(Guid tenantId, Guid taskId) => Path.Combine(root, tenantId.ToString("N"), taskId.ToString("N"));
    private static bool TryGetResultId(Guid tenantId, Guid taskId, string reference, out string resultId)
    {
        resultId = string.Empty;
        var prefix = $"worker-file:{tenantId:D}/{taskId:D}/";
        if (!reference.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        var value = reference[prefix.Length..];
        return value.Length == 32 && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F') && (resultId = value).Length > 0;
    }
}
