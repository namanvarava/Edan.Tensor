using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.Abstractions;

/// <summary>외부 전달 상태를 Tenant 범위로 영속화하는 계약입니다.</summary>
public interface IExternalDeliveryRepository
{
    /// <summary>외부 전달을 저장하거나 동일한 Tenant·Idempotency Key의 기존 전달을 원자적으로 반환합니다.</summary>
    /// <param name="tenantId">외부 전달을 소유하는 Tenant 식별자입니다.</param>
    /// <param name="targetUri">호출할 허용 대상 URI입니다.</param>
    /// <param name="payload">전달할 JSON 본문입니다.</param>
    /// <param name="idempotencyKey">동일 요청을 하나로 수렴시키는 Tenant 범위 키입니다.</param>
    /// <param name="maxAttempts">허용할 최대 전달 시도 횟수입니다.</param>
    /// <param name="nowUtc">새 전달의 생성 시각으로 사용할 UTC 시각입니다.</param>
    /// <param name="cancellationToken">저장 작업을 취소할 토큰입니다.</param>
    /// <returns>새 전달인지 기존 전달인지와 전달 상태를 함께 반환합니다.</returns>
    Task<ExternalDeliveryCreateResult> CreateOrGetAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);

    Task<ExternalDeliverySnapshot> CreateAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
    Task<ExternalDeliverySnapshot?> GetAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default);
    Task<ExternalDeliverySnapshot?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey,
        CancellationToken cancellationToken = default);
    /// <summary>아직 실행되지 않은 외부 전달을 Worker 등록 실패 보상으로 제거합니다.</summary>
    Task<bool> DeleteUnqueuedAsync(Guid tenantId, Guid deliveryId, string idempotencyKey,
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

/// <summary>외부 전달 저장 결과와 원자적 생성 여부를 표현합니다.</summary>
/// <param name="Delivery">새로 저장했거나 기존 멱등 요청에서 찾은 전달입니다.</param>
/// <param name="Created">이번 호출이 전달 레코드를 새로 저장했으면 <c>true</c>입니다.</param>
public sealed record ExternalDeliveryCreateResult(ExternalDeliverySnapshot Delivery, bool Created);
