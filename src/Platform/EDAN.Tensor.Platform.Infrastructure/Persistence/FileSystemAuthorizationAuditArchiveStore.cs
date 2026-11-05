using EDAN.Tensor.Platform.Application.Abstractions;

namespace EDAN.Tensor.Platform.Infrastructure.Persistence;

/// <summary>로컬 파일 시스템에 감사 아카이브를 식별자 기반으로 저장합니다.</summary>
public sealed class FileSystemAuthorizationAuditArchiveStore(string rootPath) : IAuthorizationAuditArchiveStore
{
    private readonly string _rootPath = Path.GetFullPath(
        string.IsNullOrWhiteSpace(rootPath) ? throw new ArgumentException("아카이브 경로가 필요합니다.") : rootPath);

    /// <summary>동일 식별자의 덮어쓰기를 금지하고 새 아카이브를 원자적으로 기록합니다.</summary>
    public async Task WriteAsync(Guid archiveId, ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        if (archiveId == Guid.Empty)
            throw new ArgumentException("아카이브 식별자는 비어 있을 수 없습니다.");
        Directory.CreateDirectory(_rootPath);
        var path = GetPath(archiveId);
        await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.WriteThrough);
        await stream.WriteAsync(content, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    /// <summary>아카이브 파일을 읽고 파일이 없으면 null을 반환합니다.</summary>
    public async Task<byte[]?> ReadAsync(Guid archiveId, CancellationToken cancellationToken = default)
    {
        if (archiveId == Guid.Empty)
            throw new ArgumentException("아카이브 식별자는 비어 있을 수 없습니다.");
        var path = GetPath(archiveId);
        if (!File.Exists(path))
            return null;
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }

    private string GetPath(Guid archiveId) =>
        Path.Combine(_rootPath, $"{archiveId:N}.edan-audit.json");
}
