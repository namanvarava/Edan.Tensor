namespace EDAN.Tensor.ApiHost.Security;

/// <summary>Development Bypass가 외부 비운영 IdP 경계를 흉내 낼 때 사용하는 고정 프로필을 제공합니다.</summary>
/// <remarks>
/// 이 프로필은 RFC 예약 도메인과 명시적인 개발 식별자만 사용하며 실제 IdP에 연결하지 않습니다.
/// 프로필은 Development Bypass가 활성화된 경우에만 사용하고, External 인증의 Metadata·JWKS·Issuer·Audience 검증을 대체하지 않습니다.
/// 키 회전 권한 값은 외부 IdP에서 실제 권한을 부여했다는 뜻이 아니라 개발 시나리오의 고정 식별자입니다.
/// </remarks>
internal sealed record DevelopmentBypassExternalIdentityProfile(
    string MetadataAddress,
    string JwksAddress,
    string Issuer,
    string Audience,
    string TestTenant,
    string TestUser,
    string StableSubject,
    string KeyRotationPermission)
{
    /// <summary>개발 Bypass가 사용하는 고정된 비운영 IdP 시뮬레이션 프로필을 반환합니다.</summary>
    public static DevelopmentBypassExternalIdentityProfile Fixed { get; } = new(
        "https://auth-test.example.com/.well-known/openid-configuration",
        "https://auth-test.example.com/jwks",
        "https://auth-test.example.com",
        "edan-tensor-api",
        "EDAN-NONPROD",
        "edan-nonprod-test-user",
        "edan-nonprod-test-subject",
        "nonprod-idp-test-key-rotation");
}
