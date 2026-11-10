using EDAN_Tensor_Licensing_Contracts;

namespace EDAN_Tensor_Licensing_Application.Services;

/// <summary>라이선스가 실행 환경의 사용 범위를 벗어나지 않는지 판단합니다.</summary>
public static class LicenseEnvironmentPolicy
{
    /// <summary>운영 환경에서 개발 전용 라이선스를 거부합니다.</summary>
    /// <param name="isDevelopmentEnvironment">현재 Host가 Development 환경인지 나타냅니다.</param>
    /// <param name="descriptor">검증된 라이선스의 공개 정보입니다.</param>
    /// <returns>현재 환경에서 라이선스를 사용할 수 있으면 <see langword="true"/>를 반환합니다.</returns>
    public static bool IsAllowed(bool isDevelopmentEnvironment, LicenseDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return isDevelopmentEnvironment
            || !string.Equals(
                descriptor.SiteOrTenant,
                DevelopmentLicenseConstants.TenantCode,
                StringComparison.Ordinal);
    }
}
