namespace EDAN.Tensor.Platform.Application.Abstractions;

/// <summary>감사 아카이브 저장소는 식별자 기반으로 아카이브 파일을 보관하고 읽습니다.</summary>
public interface IAuthorizationAuditArchiveStore
{
    /// <summary>아카이브 바이트를 새 식별자로 저장합니다.</summary>
    Task WriteAsync(Guid archiveId, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default);

    /// <summary>아카이브 바이트를 읽으며 존재하지 않으면 null을 반환합니다.</summary>
    Task<byte[]?> ReadAsync(Guid archiveId, CancellationToken cancellationToken = default);
}
