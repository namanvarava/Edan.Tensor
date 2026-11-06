namespace EDAN.Tensor.Worker.Domain.Model;

/// <summary>외부 전달의 영속 상태입니다.</summary>
public enum ExternalDeliveryStatus { Pending, Sending, Succeeded, Failed }

/// <summary>Tenant가 요청한 외부 HTTP 전달과 재시도 상태를 보유합니다.</summary>
public sealed class ExternalDelivery
{
    private ExternalDelivery() { }

    private ExternalDelivery(Guid id, Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) throw new ArgumentException("외부 전달 식별자는 비어 있을 수 없습니다.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant 식별자는 비어 있을 수 없습니다.", nameof(tenantId));
        TargetUri = ValidateUri(targetUri);
        Payload = ValidatePayload(payload);
        IdempotencyKey = ValidateText(idempotencyKey, 200, nameof(idempotencyKey));
        if (maxAttempts is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        Id = id;
        TenantId = tenantId;
        MaxAttempts = maxAttempts;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        Status = ExternalDeliveryStatus.Pending;
        Version = 1;
    }

    /// <summary>외부 전달을 생성하고 수신 측 중복 제거 키를 고정합니다.</summary>
    public static ExternalDelivery Create(Guid tenantId, string targetUri, string payload,
        string idempotencyKey, int maxAttempts, DateTimeOffset createdAtUtc) =>
        new(Guid.NewGuid(), tenantId, targetUri, payload, idempotencyKey, maxAttempts, createdAtUtc);

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string TargetUri { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public ExternalDeliveryStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public string? LastErrorCode { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public long Version { get; private set; }

    /// <summary>외부 호출 직전에 하나의 재시도 시도를 예약합니다.</summary>
    public bool BeginAttempt()
    {
        if (Status is ExternalDeliveryStatus.Succeeded or ExternalDeliveryStatus.Failed)
            return false;
        AttemptCount++;
        Status = ExternalDeliveryStatus.Sending;
        Version++;
        return true;
    }

    /// <summary>수신 응답을 확인한 성공 상태를 저장합니다.</summary>
    public void MarkSucceeded(DateTimeOffset completedAtUtc)
    {
        Status = ExternalDeliveryStatus.Succeeded;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        LastErrorCode = null;
        Version++;
    }

    /// <summary>재시도 가능한 장애를 기록하고 다음 Worker 시도를 대기합니다.</summary>
    public void MarkTransientFailure(string errorCode)
    {
        LastErrorCode = ValidateText(errorCode, 200, nameof(errorCode));
        Status = AttemptCount >= MaxAttempts ? ExternalDeliveryStatus.Failed : ExternalDeliveryStatus.Pending;
        Version++;
    }

    /// <summary>재시도하지 않을 HTTP 응답을 종결 상태로 기록합니다.</summary>
    public void MarkPermanentFailure(string errorCode, DateTimeOffset completedAtUtc)
    {
        LastErrorCode = ValidateText(errorCode, 200, nameof(errorCode));
        Status = ExternalDeliveryStatus.Failed;
        CompletedAtUtc = completedAtUtc.ToUniversalTime();
        Version++;
    }

    private static string ValidateUri(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || uri.UserInfo.Length > 0)
            throw new ArgumentException("외부 전달 주소는 사용자 정보가 없는 HTTP 또는 HTTPS 절대 URI여야 합니다.", nameof(value));
        if (value.Trim().Length > 2_048)
            throw new ArgumentException("외부 전달 주소는 2048자를 초과할 수 없습니다.", nameof(value));
        return uri.AbsoluteUri;
    }

    private static string ValidatePayload(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > 1_048_576)
            throw new ArgumentException("외부 전달 본문은 1 MiB를 초과할 수 없습니다.", nameof(value));
        return value;
    }

    private static string ValidateText(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"값은 {maxLength}자를 초과할 수 없습니다.", parameterName);
        return normalized;
    }
}
