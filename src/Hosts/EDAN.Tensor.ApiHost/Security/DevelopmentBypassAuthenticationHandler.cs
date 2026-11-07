using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>개발 환경에서 외부 IdP 없이 고정된 로컬 주체를 인증합니다.</summary>
/// <remarks>
/// 이 Handler는 <c>Authentication:Mode=Bypass</c>일 때만 등록됩니다.
/// Bypass는 외부 JWT 검증만 생략하며, Tenant 소속·조직 범위·관리자 권한 검사는 기존 정책을 그대로 적용합니다.
/// 운영 환경에서 Bypass를 선택하면 호스트가 시작되지 않도록 등록 단계에서 차단합니다.
/// </remarks>
internal sealed class DevelopmentBypassAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>요청을 고정된 개발 주체로 인증합니다.</summary>
    /// <returns>구성된 주체와 플랫폼 관리자 역할을 포함한 인증 티켓을 반환합니다.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var subject = configuration["Authentication:BypassSubject"]?.Trim();
        if (string.IsNullOrWhiteSpace(subject))
            subject = "local-development-subject";

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
