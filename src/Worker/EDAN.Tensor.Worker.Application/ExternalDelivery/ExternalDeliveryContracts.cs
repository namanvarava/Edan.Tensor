namespace EDAN.Tensor.Worker.Application.ExternalDelivery;

/// <summary>외부 전달 작업의 유형과 불투명 Payload 참조를 정의합니다.</summary>
public static class ExternalDeliveryTaskContract
{
    public const string TaskType = "external.delivery.v1";

    public static string CreatePayloadReference(Guid deliveryId)
    {
        if (deliveryId == Guid.Empty) throw new ArgumentException("외부 전달 식별자는 비어 있을 수 없습니다.", nameof(deliveryId));
        return $"external-delivery:v1:{deliveryId:N}";
    }

    public static bool TryParsePayloadReference(string payloadReference, out Guid deliveryId)
    {
        deliveryId = default;
        const string prefix = "external-delivery:v1:";
        return !string.IsNullOrWhiteSpace(payloadReference)
            && payloadReference.StartsWith(prefix, StringComparison.Ordinal)
            && Guid.TryParseExact(payloadReference[prefix.Length..], "N", out deliveryId)
            && deliveryId != Guid.Empty;
    }
}

/// <summary>외부 전달 주소와 로컬 테스트 수신기 허용 정책입니다.</summary>
public sealed record ExternalDeliveryOptions(
    IReadOnlySet<string> AllowedHosts,
    bool AllowLoopback,
    TimeSpan Timeout)
{
    /// <summary>호스트 환경에 맞는 외부 전달 옵션을 생성합니다.</summary>
    /// <param name="allowedHosts">운영 환경에서 호출을 허용할 호스트 이름 집합입니다.</param>
    /// <param name="allowLoopback">로컬 loopback 수신기를 허용할지 나타냅니다.</param>
    /// <param name="timeout">외부 전달 요청의 제한 시간입니다.</param>
    /// <param name="isDevelopment">현재 호스트가 Development 환경인지 나타냅니다.</param>
    /// <returns>환경 제한을 통과한 외부 전달 옵션을 반환합니다.</returns>
    /// <exception cref="InvalidOperationException">Development가 아닌 환경에서 loopback 허용을 요청하면 발생합니다.</exception>
    /// <remarks>
    /// loopback 허용은 개인 개발과 격리된 테스트 수신기에서만 사용합니다.
    /// 운영 환경에서 잘못된 설정이 적용되면 호스트를 시작하지 않아 SSRF 경계를 닫습니다.
    /// </remarks>
    public static ExternalDeliveryOptions Create(
        IReadOnlySet<string> allowedHosts, bool allowLoopback, TimeSpan timeout, bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(allowedHosts);
        if (allowLoopback && !isDevelopment)
            throw new InvalidOperationException(
                "ExternalDelivery:AllowLoopback=true는 Development 환경에서만 사용할 수 있습니다.");

        return new ExternalDeliveryOptions(allowedHosts, allowLoopback, timeout);
    }

    public static ExternalDeliveryOptions Default { get; } = new(
        new HashSet<string>(StringComparer.OrdinalIgnoreCase), false, TimeSpan.FromSeconds(30));
}

/// <summary>외부 응답을 받지 못해 동일 키로 재시도해야 하는 장애입니다.</summary>
public sealed class ExternalDeliveryTransientException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
