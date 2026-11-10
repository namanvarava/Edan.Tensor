using System.Text.Json.Serialization;

namespace EDAN_Tensor_Licensing_Contracts;

/// <summary>라이선스의 업무 상태를 나타냅니다.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LicenseStatus
{
    /// <summary>라이선스가 현재 사용 가능한 상태입니다.</summary>
    Active,

    /// <summary>라이선스의 유효 기간이 끝난 상태입니다.</summary>
    Expired,

    /// <summary>발급된 라이선스가 권한자에 의해 폐기된 상태입니다.</summary>
    Revoked,

    /// <summary>라이선스의 시작 시각이 아직 도래하지 않은 상태입니다.</summary>
    NotYetValid
}

/// <summary>서명된 라이선스에 포함되는 공개 계약을 나타냅니다.</summary>
public sealed record LicenseDescriptor(
    Guid LicenseId,
    string SiteOrTenant,
    string Product,
    IReadOnlyList<string> Features,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string KeyId,
    LicenseStatus Status);

/// <summary>서명된 라이선스와 해석된 공개 정보를 함께 반환합니다.</summary>
public sealed record IssuedLicense(
    string Token,
    LicenseDescriptor Descriptor);

/// <summary>라이선스 검증에 필요한 대상과 기능을 지정합니다.</summary>
public sealed record LicenseValidationRequest(
    string SiteOrTenant,
    string Product,
    IReadOnlyCollection<string> RequiredFeatures);

/// <summary>라이선스 검증 결과와 실패 사유를 나타냅니다.</summary>
public sealed record LicenseValidationResult(
    bool IsValid,
    string? ErrorCode,
    string? ErrorDescription,
    LicenseDescriptor? Descriptor);

/// <summary>개발 라이선스 발급 API가 반환하는 공개 응답입니다.</summary>
public sealed record DevelopmentLicenseResponse(
    string Token,
    LicenseDescriptor Descriptor);

/// <summary>라이선스 공개키를 JWK 형식으로 반환하는 응답입니다.</summary>
public sealed record LicensePublicKeyResponse(
    string KeyId,
    string Jwk);

/// <summary>라이선스 폐기 API가 반환하는 공개 응답입니다.</summary>
public sealed record LicenseRevocationResponse(
    Guid LicenseId,
    LicenseStatus Status);

/// <summary>라이선스 서명 토큰에 사용할 표준 Claim 이름을 제공합니다.</summary>
public static class LicenseClaimNames
{
    /// <summary>라이선스 식별자를 나타내는 Claim 이름입니다.</summary>
    public const string LicenseId = "license_id";

    /// <summary>라이선스 대상 Site 또는 Tenant를 나타내는 Claim 이름입니다.</summary>
    public const string SiteOrTenant = "site_or_tenant";

    /// <summary>라이선스가 허용하는 제품을 나타내는 Claim 이름입니다.</summary>
    public const string Product = "product";

    /// <summary>라이선스가 허용하는 기능 배열을 나타내는 Claim 이름입니다.</summary>
    public const string Features = "features";

    /// <summary>라이선스 상태를 나타내는 Claim 이름입니다.</summary>
    public const string Status = "status";

    /// <summary>발급자 식별자를 나타내는 표준 Claim 이름입니다.</summary>
    public const string Issuer = "iss";

    /// <summary>발급 시각을 나타내는 표준 Claim 이름입니다.</summary>
    public const string IssuedAt = "iat";

    /// <summary>사용 시작 시각을 나타내는 표준 Claim 이름입니다.</summary>
    public const string NotBefore = "nbf";

    /// <summary>만료 시각을 나타내는 표준 Claim 이름입니다.</summary>
    public const string Expiration = "exp";

    /// <summary>KeyId를 나타내는 JWT Header 이름입니다.</summary>
    public const string KeyId = "kid";
}

