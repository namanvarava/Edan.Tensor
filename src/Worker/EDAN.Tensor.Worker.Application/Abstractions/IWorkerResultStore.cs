namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>Worker raw result bytes의 저장소 계약입니다. 작업 DB에는 이 계약이 반환한 참조만 저장합니다.</summary>
public interface IWorkerResultStore
{
    Task<WorkerResultMetadata> SaveAsync(Guid tenantId, Guid taskId, string contentType, Stream content,
        CancellationToken cancellationToken = default);
    Task<WorkerResultMetadata?> GetMetadataAsync(Guid tenantId, Guid taskId, string resultReference,
        CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(WorkerResultMetadata metadata, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkerResultMetadata metadata, CancellationToken cancellationToken = default);
}

public sealed record WorkerResultMetadata(Guid TenantId, Guid TaskId, string ResultReference,
    string ContentType, long Length, string Sha256, DateTimeOffset CreatedAtUtc);
