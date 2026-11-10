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
    /// <exception cref="IOException">라이선스 저장소에 접근할 수 없고 다른 요청이 저장한 라이선스도 확인할 수 없을 때 발생합니다.</exception>
    public async Task<Result<IssuedLicense>> IssueOrGetAsync(
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await licenseStore.GetAsync(cancellationToken);
        if (existing is not null)
            return ValidateStoredLicense(existing, nowUtc);

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
        try
        {
            await licenseStore.SaveAsync(issued, cancellationToken);
            return Result<IssuedLicense>.Success(issued);
        }
        catch (IOException)
        {
            // 다른 발급 요청이 먼저 저장한 경우에는 저장 경쟁을 정상적인 재사용으로 수렴시킵니다.
            var racedExisting = await licenseStore.GetAsync(cancellationToken);
            if (racedExisting is null)
                throw;

            return ValidateStoredLicense(racedExisting, nowUtc);
        }
    }

    private Result<IssuedLicense> ValidateStoredLicense(IssuedLicense existing, DateTimeOffset nowUtc)
    {
        var existingValidation = tokenCodec.ReadAndValidate(existing.Token, nowUtc);
        if (existingValidation.IsSuccess
            && IsApprovedDevelopmentLicense(existingValidation.Value))
            return Result<IssuedLicense>.Success(existing);

        return Result<IssuedLicense>.Failure(new Error(
            "licensing.development.license_unusable",
            "저장된 개발 라이선스가 승인된 계약에 맞지 않거나 더 이상 유효하지 않습니다."));
    }

    private static bool IsApprovedDevelopmentLicense(LicenseDescriptor descriptor) =>
        descriptor.SiteOrTenant == DevelopmentLicenseConstants.TenantCode
        && descriptor.Product == DevelopmentLicenseConstants.Product
        && descriptor.Status == LicenseStatus.Active
        && descriptor.Features.SequenceEqual(DevelopmentLicenseConstants.Features, StringComparer.Ordinal)
        && descriptor.ExpiresAtUtc == descriptor.StartsAtUtc.AddYears(DevelopmentLicenseConstants.ValidityYears);
}

