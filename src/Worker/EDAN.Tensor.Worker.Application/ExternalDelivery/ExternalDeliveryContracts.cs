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
    public static ExternalDeliveryOptions Default { get; } = new(
        new HashSet<string>(StringComparer.OrdinalIgnoreCase), false, TimeSpan.FromSeconds(30));
}

/// <summary>외부 응답을 받지 못해 동일 키로 재시도해야 하는 장애입니다.</summary>
public sealed class ExternalDeliveryTransientException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
