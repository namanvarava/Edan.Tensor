using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>JWT 인증과 플랫폼 역할 기반 인가 정책을 등록합니다.</summary>
/// <remarks>
/// API는 토큰을 발급하지 않고 외부 IdP가 발급한 토큰의 Issuer, Audience, 만료 시간, 서명을 검증합니다.
/// 비운영 외부 검증에서는 Edan.Identity와 같은 별도 OIDC 서비스를 사용하고,
/// <c>Authentication:MetadataAddress</c>에 <c>https://auth-test.&lt;소유한도메인&gt;/.well-known/openid-configuration</c> 형식의 주소를 지정합니다.
/// Metadata 응답의 <c>jwks_uri</c>가 공개 서명 키의 기준 주소가 되며, API는 새 키를 다시 조회할 수 있어야 합니다.
/// Issuer는 토큰의 <c>iss</c> 값과 정확히 일치하는 <c>https://auth-test.&lt;소유한도메인&gt;</c> 형식을 사용하고,
/// Audience는 API의 고정 식별자인 <c>edan-tensor-api</c>를 사용합니다.
/// <c>Authentication:RequireHttpsMetadata=false</c>는 로컬 HTTP 테스트에서만 허용하고 비운영·운영 외부 환경에서는 HTTPS를 사용합니다.
/// 개인키, 클라이언트 비밀값, 비밀번호와 액세스 토큰은 코드·문서·로그·저장소에 기록하지 않습니다.
/// </remarks>
public static class PlatformAuthenticationExtensions
{
    /// <summary>서명 검증을 수행하는 JWT 인증 및 Tenant·관리자 정책을 등록합니다.</summary>
    /// <param name="services">인증과 인가 서비스를 등록할 서비스 컬렉션을 지정합니다.</param>
    /// <param name="configuration">Issuer, Audience, Metadata 주소와 HTTPS 정책을 읽을 구성 객체를 지정합니다.</param>
    /// <returns>인증과 인가 정책을 추가한 서비스 컬렉션을 반환합니다.</returns>
    /// <remarks>
    /// Metadata 주소가 있으면 JwtBearer가 JWKS를 조회하고 알 수 없는 <c>kid</c>를 발견할 때 키를 새로 조회합니다.
    /// Issuer 또는 Audience가 누락되어도 검증 조건은 꺼지지 않으므로 보호 API는 닫힌 상태를 유지합니다.
    /// 테스트 Tenant의 사용자는 이메일이 아닌 IdP의 안정적인 <c>sub</c> 값으로 Tenant 멤버십에 연결해야 합니다.
    /// </remarks>
    public static IServiceCollection AddPlatformSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Authentication:Issuer"];
        var audience = configuration["Authentication:Audience"];
        var signingKey = configuration["Authentication:SigningKey"];
        var metadataAddress = configuration["Authentication:MetadataAddress"];
        var requireHttpsMetadata = configuration.GetValue("Authentication:RequireHttpsMetadata", true);
        // 비운영 외부 IdP는 API와 별도 서비스로 배포하고 Edan.Identity의 테스트 주소를 사용합니다.
        // 권장 값은 MetadataAddress=`https://auth-test.<소유한도메인>/.well-known/openid-configuration`,
        // Issuer=`https://auth-test.<소유한도메인>`, Audience=`edan-tensor-api`입니다.
        // Metadata가 반환하는 jwks_uri를 사용하며 RS256 비대칭 키와 겹침 구간이 있는 키 회전을 유지합니다.
        // 필수 설정이 누락되어도 모든 JWT 검증 조건을 유지하여 보호 API가 계속 닫힌 상태를 유지합니다.
        var key = !string.IsNullOrWhiteSpace(signingKey) && Encoding.UTF8.GetByteCount(signingKey) >= 32
            ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
            : null;

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, RequestTenantContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                if (!string.IsNullOrWhiteSpace(metadataAddress))
                {
                    options.MetadataAddress = metadataAddress;
                    options.RequireHttpsMetadata = requireHttpsMetadata;
                    options.RefreshOnIssuerKeyNotFound = true;
                }
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantMember", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("sub"));
            options.AddPolicy("PlatformAdministrator", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("sub").RequireRole("platform_admin"));
        });

        return services;
    }
}

