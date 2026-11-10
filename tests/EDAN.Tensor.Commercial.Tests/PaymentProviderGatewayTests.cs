using EDAN.Tensor.SharedKernel.Results;
using EDAN_Tensor_Commercial_Application;
using EDAN_Tensor_Commercial_Contracts;
using Xunit;

namespace EDAN_Tensor_Commercial_Tests;

/// <summary>공급자 미선정 결제 경계와 교체 가능한 호출 경계를 검증합니다.</summary>
public sealed class PaymentProviderGatewayTests
{
    /// <summary>공급자가 선정되지 않으면 결제 성공을 반환하지 않습니다.</summary>
    [Fact]
    public async Task CreateCheckout_RejectsWhenProviderIsNotSelected()
    {
        var gateway = new PaymentProviderGateway(null);

        var result = await gateway.CreateCheckoutAsync(
            new PaymentCheckoutRequest("EDAN-NONPROD", "tensor-development", "request-1"));

        Assert.False(result.IsSuccess);
        Assert.Equal(CommercialErrorCodes.PaymentProviderNotSelected, result.Error.Code);
    }

    /// <summary>선정된 공급자는 가격을 확정하지 않은 요청을 교체 가능한 경계로 전달합니다.</summary>
    [Fact]
    public async Task CreateCheckout_ForwardsToSelectedProvider()
    {
        var provider = new RecordingPaymentProvider();
        var gateway = new PaymentProviderGateway(provider);
        var request = new PaymentCheckoutRequest("EDAN-NONPROD", "tensor-development", "request-2");

        var result = await gateway.CreateCheckoutAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, provider.CallCount);
        Assert.Same(request, provider.LastRequest);
        Assert.Equal(PaymentOperationStatus.Pending, result.Value.Status);
    }

    private sealed class RecordingPaymentProvider : IPaymentProvider
    {
        public string ProviderCode => "test-provider";

        public int CallCount { get; private set; }

        public PaymentCheckoutRequest? LastRequest { get; private set; }

        public Task<Result<PaymentOperation>> CreateCheckoutAsync(
            PaymentCheckoutRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastRequest = request;
            return Task.FromResult(Result<PaymentOperation>.Success(
                new PaymentOperation(ProviderCode, PaymentOperationStatus.Pending, "test-reference")));
        }
    }
}
