using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Contracts;

namespace EDAN_Tensor_Licensing_Application.Services;

/// <summary>서명된 라이선스가 요청 Tenant·제품·기능과 현재 상태를 만족하는지 평가합니다.</summary>
public sealed class LicenseValidationService(
    ILicenseTokenCodec tokenCodec,
    ILicenseStatusStore statusStore)
{
    /// <summary>토큰의 서명·기간·폐기·Tenant·제품·기능 조건을 모두 검증합니다.</summary>
    /// <param name="token">검증할 compact JWS 토큰입니다.</param>
    /// <param name="request">허용되어야 하는 Tenant·제품·기능 조건입니다.</param>
    /// <param name="nowUtc">검증 기준 UTC 시각입니다.</param>
    /// <param name="cancellationToken">작업을 중단할 취소 토큰입니다.</param>
    /// <returns>성공 여부와 공개 라이선스 정보를 반환합니다.</returns>
    public async Task<LicenseValidationResult> ValidateAsync(
        string token,
        LicenseValidationRequest request,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Invalid("licensing.token.required", "라이선스 토큰이 필요합니다.");
        if (string.IsNullOrWhiteSpace(request.SiteOrTenant)
            || string.IsNullOrWhiteSpace(request.Product)
            || request.RequiredFeatures.Any(string.IsNullOrWhiteSpace))
            return Invalid("licensing.request.invalid", "라이선스 검증 대상이 올바르지 않습니다.");

        var parsed = tokenCodec.ReadAndValidate(token, nowUtc);
        if (parsed.IsFailure)
            return Invalid(parsed.Error.Code, parsed.Error.Description);

        var descriptor = parsed.Value;
        var storedStatus = await statusStore.GetAsync(descriptor.LicenseId, cancellationToken);
        if (storedStatus == LicenseStatus.Revoked || descriptor.Status == LicenseStatus.Revoked)
            return Invalid("licensing.revoked", "라이선스가 폐기되었습니다.", descriptor);
        if (descriptor.SiteOrTenant != request.SiteOrTenant)
            return Invalid("licensing.tenant_mismatch", "라이선스 대상 Tenant가 일치하지 않습니다.", descriptor);
        if (descriptor.Product != request.Product)
            return Invalid("licensing.product_mismatch", "라이선스 제품이 일치하지 않습니다.", descriptor);
        if (!request.RequiredFeatures.All(feature =>
                descriptor.Features.Contains(feature, StringComparer.Ordinal)))
            return Invalid("licensing.feature_not_entitled", "요청한 기능이 라이선스에 포함되지 않았습니다.", descriptor);

        return new LicenseValidationResult(true, null, null, descriptor);
    }

    private static LicenseValidationResult Invalid(
        string code,
        string description,
        LicenseDescriptor? descriptor = null) =>
        new(false, code, description, descriptor);
}

