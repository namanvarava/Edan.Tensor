using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>개발 환경에서 외부 IdP 없이 모든 요청을 고정된 로컬 주체로 인증합니다.</summary>
/// <remarks>
/// 이 Handler는 <c>Authentication:Mode=Bypass</c>일 때만 등록됩니다.
/// 요청에 Bearer 토큰이 없어도 인증된 ClaimsPrincipal을 만들며 외부 Metadata, Issuer, Audience와 JWKS를 읽지 않습니다.
/// 고정된 개발용 비운영 IdP 시뮬레이션 프로필을 로그로 남기지만 해당 주소에 연결하거나 외부 서명을 검증하지 않습니다.
/// Bypass는 인증 절차만 대체하며, Tenant 소속·조직 범위·관리자 권한 검사는 기존 정책을 그대로 적용합니다.
/// 운영 환경에서 Bypass를 선택하면 호스트가 시작되지 않도록 등록 단계에서 차단합니다.
/// </remarks>
internal sealed class DevelopmentBypassAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration,
    DevelopmentBypassExternalIdentityProfile profile)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private static int _profileWarningWritten;

    /// <summary>토큰 유무와 관계없이 요청을 고정된 개발 주체로 인증합니다.</summary>
    /// <returns>항상 인증 성공한 개발 주체의 인증 티켓을 반환합니다.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Interlocked.Exchange(ref _profileWarningWritten, 1) == 0)
        {
            Logger.LogWarning(
                "Development Bypass가 실제 외부 IdP 대신 고정 시뮬레이션 프로필을 사용합니다. MetadataAddress={MetadataAddress}, JwksAddress={JwksAddress}, Issuer={Issuer}, Audience={Audience}, TestTenant={TestTenant}, TestUser={TestUser}, StableSubject={StableSubject}, KeyRotationPermission={KeyRotationPermission}",
                profile.MetadataAddress, profile.JwksAddress, profile.Issuer, profile.Audience,
                profile.TestTenant, profile.TestUser, profile.StableSubject, profile.KeyRotationPermission);
        }

        var subject = configuration["Authentication:BypassSubject"]?.Trim();
        if (string.IsNullOrWhiteSpace(subject))
            subject = profile.StableSubject;

        var role = configuration["Authentication:BypassRole"]?.Trim();
        if (string.IsNullOrWhiteSpace(role))
            role = "platform_admin";

        var claims = new[]
        {
            new Claim("sub", subject),
            new Claim("role", role)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name, "sub", "role");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
