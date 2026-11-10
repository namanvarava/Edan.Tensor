using EDAN_Tensor_Licensing_Application.Abstractions;
using EDAN_Tensor_Licensing_Contracts;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN_Tensor_Licensing_Application.Services;

/// <summary>승인된 고정값으로 개발 라이선스를 한 번 발급하거나 기존 공개 토큰을 반환합니다.</summary>
public sealed class DevelopmentLicenseService(
    IDevelopmentLicenseStore licenseStore,
    ILicenseTokenCodec tokenCodec)
{
    /// <summary>개발 라이선스를 발급하고 동일한 저장 결과를 재사용합니다.</summary>
    /// <param name="nowUtc">발급과 유효 기간 계산에 사용할 UTC 시각입니다.</param>
    /// <param name="cancellationToken">작업을 중단할 취소 토큰입니다.</param>
    /// <returns>5년 유효한 개발 라이선스 또는 저장·계약 오류를 반환합니다.</returns>
    public async Task<Result<IssuedLicense>> IssueOrGetAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await licenseStore.GetAsync(cancellationToken);
        if (existing is not null)
        {
            var existingValidation = tokenCodec.ReadAndValidate(existing.Token, nowUtc);
            if (existingValidation.IsSuccess
                && IsApprovedDevelopmentLicense(existingValidation.Value))
                return Result<IssuedLicense>.Success(existing);

            return Result<IssuedLicense>.Failure(new Error(
                "licensing.development.license_unusable",
                "저장된 개발 라이선스가 승인된 계약에 맞지 않거나 더 이상 유효하지 않습니다."));
        }

        var descriptor = new LicenseDescriptor(
            Guid.NewGuid(),
            DevelopmentLicenseConstants.TenantCode,
            DevelopmentLicenseConstants.Product,
            DevelopmentLicenseConstants.Features,
            nowUtc,
            nowUtc,
            nowUtc.AddYears(DevelopmentLicenseConstants.ValidityYears),
            tokenCodec.KeyId,
            LicenseStatus.Active);
        var issued = new IssuedLicense(tokenCodec.CreateToken(descriptor), descriptor);
        await licenseStore.SaveAsync(issued, cancellationToken);
        return Result<IssuedLicense>.Success(issued);
    }

    private static bool IsApprovedDevelopmentLicense(LicenseDescriptor descriptor) =>
        descriptor.SiteOrTenant == DevelopmentLicenseConstants.TenantCode
        && descriptor.Product == DevelopmentLicenseConstants.Product
        && descriptor.Status == LicenseStatus.Active
        && descriptor.Features.SequenceEqual(DevelopmentLicenseConstants.Features, StringComparer.Ordinal)
        && descriptor.ExpiresAtUtc == descriptor.StartsAtUtc.AddYears(DevelopmentLicenseConstants.ValidityYears);
}

