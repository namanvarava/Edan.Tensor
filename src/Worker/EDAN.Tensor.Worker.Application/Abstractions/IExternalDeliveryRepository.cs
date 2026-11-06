using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>외부 전달 상태를 Tenant 범위로 영속화하는 계약입니다.</summary>
public interface IExternalDeliveryRepository
{
    Task<ExternalDeliverySnapshot> CreateAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
    Task<ExternalDeliverySnapshot?> GetAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default);
    Task<ExternalDeliverySnapshot?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey,
        CancellationToken cancellationToken = default);
    Task<ExternalDeliverySnapshot?> BeginAttemptAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default);
    Task CompleteAsync(Guid tenantId, Guid deliveryId, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
    Task MarkTransientFailureAsync(Guid tenantId, Guid deliveryId, string errorCode,
        CancellationToken cancellationToken = default);
    Task MarkPermanentFailureAsync(Guid tenantId, Guid deliveryId, string errorCode,
        DateTimeOffset nowUtc, CancellationToken cancellationToken = default);
}

/// <summary>외부 전달 API와 Worker가 공유하는 상태 요약입니다.</summary>
public sealed record ExternalDeliverySnapshot(
    Guid Id,
    Guid TenantId,
    string TargetUri,
    string Payload,
    string IdempotencyKey,
    ExternalDeliveryStatus Status,
    int AttemptCount,
    int MaxAttempts,
    string? LastErrorCode,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
