using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>통합 테스트는 요청 헤더로 인증 주체를 구성하여 외부 인증 서버 없이 인가 경계를 검증합니다.</summary>
public sealed class IntegrationAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Subject", out var subject))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new("sub", subject.ToString()) };
        if (Request.Headers.TryGetValue("X-Test-Tenant", out var tenant))
            claims.Add(new Claim("tenant_id", tenant.ToString()));
        if (Request.Headers.TryGetValue("X-Test-Role", out var role))
            claims.Add(new Claim("role", role.ToString()));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
