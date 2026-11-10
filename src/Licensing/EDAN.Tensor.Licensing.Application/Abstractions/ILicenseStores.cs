using EDAN_Tensor_Licensing_Contracts;

namespace EDAN_Tensor_Licensing_Application.Abstractions;

/// <summary>발급된 개발 라이선스의 공개 토큰을 보관하는 경계를 제공합니다.</summary>
public interface IDevelopmentLicenseStore
{
    /// <summary>이미 발급된 개발 라이선스를 조회합니다.</summary>
    Task<IssuedLicense?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>개발 라이선스를 최초 한 번 저장합니다.</summary>
    Task SaveAsync(IssuedLicense license, CancellationToken cancellationToken = default);
}

/// <summary>라이선스의 폐기 상태를 보관하는 경계를 제공합니다.</summary>
public interface ILicenseStatusStore
{
    /// <summary>라이선스의 현재 상태를 조회합니다.</summary>
    Task<LicenseStatus?> GetAsync(Guid licenseId, CancellationToken cancellationToken = default);

    /// <summary>라이선스 상태를 폐기 상태로 변경합니다.</summary>
    Task RevokeAsync(Guid licenseId, CancellationToken cancellationToken = default);
}

