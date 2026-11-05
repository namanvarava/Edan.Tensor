using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.Platform.Application.DependencyInjection;

/// <summary>플랫폼 응용 서비스와 공통 시간 서비스를 등록합니다.</summary>
public static class PlatformApplicationExtensions
{
    /// <summary>플랫폼 생성 업무 서비스와 시스템 시계를 등록합니다.</summary>
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddScoped<PlatformProvisioningService>();
        services.AddScoped<TenantMembershipService>();
        services.AddScoped<ScopedMembershipService>();
        services.AddScoped<PlatformAuthorizationService>();
        services.AddScoped<AuthorizationPolicyService>();
        services.AddScoped<AuthorizationAuditQueryService>();
        services.AddScoped<AuthorizationAuditIntegrityService>();
        services.AddScoped<AuthorizationAuditArchiveService>();
        return services;
    }
}
