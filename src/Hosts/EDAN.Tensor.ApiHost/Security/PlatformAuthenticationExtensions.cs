using System.Security.Claims;
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
        var key = string.IsNullOrWhiteSpace(signingKey) ? null : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantContext, ClaimsTenantContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidIssuer = issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub",
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantMember", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("tenant_id").RequireClaim("sub"));
            options.AddPolicy("PlatformAdministrator", policy =>
                policy.RequireAuthenticatedUser().RequireClaim("sub").RequireRole("platform_admin"));
        });

        return services;
    }
}
