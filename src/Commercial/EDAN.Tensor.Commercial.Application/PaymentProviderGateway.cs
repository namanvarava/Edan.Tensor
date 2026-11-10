using EDAN.Tensor.SharedKernel.Results;
using EDAN_Tensor_Commercial_Contracts;

namespace EDAN_Tensor_Commercial_Application;

/// <summary>선정된 결제 공급자에게만 결제 요청을 전달하고 미선정 상태에서는 실패합니다.</summary>
/// <param name="selectedProvider">공급자 선정 전에는 결제 성공을 막기 위해 null을 허용하는 선택된 공급자입니다.</param>
public sealed class PaymentProviderGateway(IPaymentProvider? selectedProvider)
{
    /// <summary>선정된 공급자에게 결제 세션 생성을 요청합니다.</summary>
    /// <param name="request">Tenant와 내부 상품 참조를 포함한 공급자 중립 요청입니다.</param>
    /// <param name="cancellationToken">작업을 중단할 취소 토큰입니다.</param>
    /// <returns>공급자 결과를 전달하거나 공급자 미선정 오류를 반환합니다.</returns>
    public Task<Result<PaymentOperation>> CreateCheckoutAsync(
        PaymentCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (selectedProvider is null)
        {
            return Task.FromResult(Result<PaymentOperation>.Failure(new Error(
                CommercialErrorCodes.PaymentProviderNotSelected,
                "결제 공급자가 선정되기 전에는 결제를 완료하거나 유료 권한을 부여할 수 없습니다.")));
        }

        return selectedProvider.CreateCheckoutAsync(request, cancellationToken);
    }
}
