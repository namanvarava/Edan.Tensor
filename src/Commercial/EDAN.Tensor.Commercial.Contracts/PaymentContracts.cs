using EDAN.Tensor.SharedKernel.Results;

namespace EDAN_Tensor_Commercial_Contracts;

/// <summary>결제 공급자에 전달할 공급자 중립 요청을 나타냅니다.</summary>
/// <param name="TenantCode">결제를 요청한 Tenant 코드입니다.</param>
/// <param name="ProductReference">공급자별 가격을 포함하지 않는 내부 상품 참조입니다.</param>
/// <param name="RequestId">중복 요청을 구분하는 호출자 요청 식별자입니다.</param>
public sealed record PaymentCheckoutRequest(
    string TenantCode,
    string ProductReference,
    string RequestId);

/// <summary>결제 시도의 현재 상태를 나타냅니다.</summary>
public enum PaymentOperationStatus
{
    /// <summary>공급자가 결제 처리를 아직 끝내지 않았음을 나타냅니다.</summary>
    Pending,

    /// <summary>공급자가 결제를 성공적으로 처리했음을 나타냅니다.</summary>
    Succeeded,

    /// <summary>공급자가 결제를 처리하지 못했음을 나타냅니다.</summary>
    Failed
}

/// <summary>결제 공급자가 반환한 공급자 중립 결과를 나타냅니다.</summary>
/// <param name="ProviderCode">결제를 처리한 공급자 식별자입니다.</param>
/// <param name="Status">공급자가 반환한 결제 상태입니다.</param>
/// <param name="ExternalReference">공급자 외부 참조이며 없을 수 있습니다.</param>
public sealed record PaymentOperation(
    string ProviderCode,
    PaymentOperationStatus Status,
    string? ExternalReference);

/// <summary>공급자별 결제 호출을 교체 가능한 경계로 추상화합니다.</summary>
public interface IPaymentProvider
{
    /// <summary>공급자 식별자를 반환합니다.</summary>
    string ProviderCode { get; }

    /// <summary>공급자에 결제 세션 생성을 요청합니다.</summary>
    /// <param name="request">가격을 포함하지 않는 공급자 중립 요청입니다.</param>
    /// <param name="cancellationToken">작업을 중단할 취소 토큰입니다.</param>
    /// <returns>결제 시도 결과 또는 안정적인 오류를 반환합니다.</returns>
    Task<Result<PaymentOperation>> CreateCheckoutAsync(
        PaymentCheckoutRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Commercial 결제 경계에서 사용하는 안정적인 오류 코드를 제공합니다.</summary>
public static class CommercialErrorCodes
{
    /// <summary>결제 공급자가 아직 선정되지 않았음을 나타냅니다.</summary>
    public const string PaymentProviderNotSelected = "commercial.payment_provider_not_selected";
}
