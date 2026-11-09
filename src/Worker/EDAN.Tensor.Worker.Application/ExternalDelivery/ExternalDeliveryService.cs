using System.Net;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.Worker.Application.ExternalDelivery;

/// <summary>외부 전달을 Idempotency Key와 재시도 가능한 장애 구분으로 실행합니다.</summary>
public sealed class ExternalDeliveryService(
    IExternalDeliveryRepository repository,
    HttpClient httpClient,
    ExternalDeliveryOptions options)
{
    /// <summary>주소와 JSON 본문을 저장하고 Worker가 사용할 전달 참조를 반환합니다.</summary>
    public async Task<ExternalDeliverySnapshot> CreateAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        return (await CreateOrGetAsync(tenantId, targetUri, payload, idempotencyKey, maxAttempts,
            nowUtc, cancellationToken)).Delivery;
    }

    /// <summary>동일한 Tenant·Idempotency Key의 경쟁 요청을 하나의 전달로 수렴시킵니다.</summary>
    /// <param name="tenantId">외부 전달을 소유하는 Tenant 식별자입니다.</param>
    /// <param name="targetUri">호출할 허용 대상 URI입니다.</param>
    /// <param name="payload">전달할 JSON 본문입니다.</param>
    /// <param name="idempotencyKey">동일 요청을 하나로 수렴시키는 Tenant 범위 키입니다.</param>
    /// <param name="maxAttempts">허용할 최대 전달 시도 횟수입니다.</param>
    /// <param name="nowUtc">새 전달의 생성 시각으로 사용할 UTC 시각입니다.</param>
    /// <param name="cancellationToken">저장 작업을 취소할 토큰입니다.</param>
    /// <returns>전달 상태와 이번 호출의 신규 생성 여부를 반환합니다.</returns>
    public async Task<ExternalDeliveryCreateResult> CreateOrGetAsync(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateTarget(targetUri);
        return await repository.CreateOrGetAsync(tenantId, targetUri, payload, idempotencyKey,
            maxAttempts, nowUtc, cancellationToken);
    }

    /// <summary>한 번의 전달을 수행하고 응답 유실·timeout·5xx는 재시도 상태로 남깁니다.</summary>
    public async Task<ExternalDeliverySnapshot> DeliverAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetAsync(tenantId, deliveryId, cancellationToken)
            ?? throw new KeyNotFoundException("Tenant 범위에서 외부 전달을 찾을 수 없습니다.");
        if (existing.Status is ExternalDeliveryStatus.Succeeded or ExternalDeliveryStatus.Failed)
            return existing;

        var attempt = await repository.BeginAttemptAsync(tenantId, deliveryId, cancellationToken)
            ?? throw new KeyNotFoundException("Tenant 범위에서 외부 전달을 찾을 수 없습니다.");
        if (attempt.Status is ExternalDeliveryStatus.Succeeded or ExternalDeliveryStatus.Failed)
            return attempt;

        using var request = new HttpRequestMessage(HttpMethod.Post, attempt.TargetUri)
        {
            Content = new StringContent(attempt.Payload, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", attempt.IdempotencyKey);
        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if ((int)response.StatusCode is >= 200 and < 300)
            {
                await repository.CompleteAsync(tenantId, deliveryId, DateTimeOffset.UtcNow, cancellationToken);
                return (await repository.GetAsync(tenantId, deliveryId, cancellationToken))!;
            }
            if ((int)response.StatusCode is >= 400 and < 500)
            {
                await repository.MarkPermanentFailureAsync(tenantId, deliveryId,
                    "external.delivery.http_4xx", DateTimeOffset.UtcNow, CancellationToken.None);
                return (await repository.GetAsync(tenantId, deliveryId, CancellationToken.None))!;
            }

            await repository.MarkTransientFailureAsync(tenantId, deliveryId,
                "external.delivery.http_5xx", CancellationToken.None);
            throw new ExternalDeliveryTransientException(
                "external.delivery.http_5xx", "외부 수신기가 5xx 응답을 반환했습니다.");
        }
        catch (ExternalDeliveryTransientException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            await repository.MarkTransientFailureAsync(tenantId, deliveryId,
                "external.delivery.timeout", CancellationToken.None);
            throw new ExternalDeliveryTransientException(
                "external.delivery.timeout", "외부 수신기 응답 시간이 초과했습니다.");
        }
        catch (HttpRequestException)
        {
            await repository.MarkTransientFailureAsync(tenantId, deliveryId,
                "external.delivery.response_lost", CancellationToken.None);
            throw new ExternalDeliveryTransientException(
                "external.delivery.response_lost", "외부 수신기 응답을 확인하지 못했습니다.");
        }
    }

    /// <summary>Tenant 범위의 외부 전달 상태를 조회합니다.</summary>
    public Task<ExternalDeliverySnapshot?> GetAsync(Guid tenantId, Guid deliveryId,
        CancellationToken cancellationToken = default) => repository.GetAsync(tenantId, deliveryId, cancellationToken);

    /// <summary>Tenant 범위에서 Idempotency Key로 기존 전달을 찾습니다.</summary>
    public Task<ExternalDeliverySnapshot?> GetByIdempotencyKeyAsync(Guid tenantId, string idempotencyKey,
        CancellationToken cancellationToken = default) => repository.GetByIdempotencyKeyAsync(
            tenantId, idempotencyKey, cancellationToken);

    /// <summary>Worker 등록에 실패한 새 외부 전달만 Tenant 범위에서 보상 제거합니다.</summary>
    public Task<bool> DeleteUnqueuedAsync(Guid tenantId, Guid deliveryId, string idempotencyKey,
        CancellationToken cancellationToken = default) => repository.DeleteUnqueuedAsync(
            tenantId, deliveryId, idempotencyKey, cancellationToken);

    private void ValidateTarget(string targetUri)
    {
        if (!Uri.TryCreate(targetUri, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || uri.UserInfo.Length > 0)
            throw new ArgumentException("외부 전달 주소는 HTTP 또는 HTTPS 절대 URI여야 합니다.", nameof(targetUri));
        var isLoopback = uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || IPAddress.TryParse(uri.Host, out var address) && IPAddress.IsLoopback(address);
        if (!(isLoopback && options.AllowLoopback)
            && !options.AllowedHosts.Contains(uri.Host))
            throw new ArgumentException("외부 전달 주소가 허용된 수신기 목록에 없습니다.", nameof(targetUri));
    }
}
