using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>JWT 인증과 플랫폼 역할 기반 인가 정책을 등록합니다.</summary>
public static class PlatformAuthenticationExtensions
{
    /// <summary>서명 검증을 수행하는 JWT 인증 및 Tenant·관리자 정책을 등록합니다.</summary>
    public static IServiceCollection AddPlatformSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Authentication:Issuer"];
        var audience = configuration["Authentication:Audience"];
        var signingKey = configuration["Authentication:SigningKey"];
        // 필수 설정이 누락되어도 모든 JWT 검증 조건을 유지하여 보호 API가 계속 닫힌 상태를 유지합니다.
        var key = !string.IsNullOrWhiteSpace(signingKey) && Encoding.UTF8.GetByteCount(signingKey) >= 32
            ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
            : null;

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, RequestTenantContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
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

