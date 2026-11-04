using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EDAN.Tensor.Platform.Infrastructure.DependencyInjection;

/// <summary>호스트가 선택한 EF Core 공급자 옵션으로 플랫폼 저장소를 등록합니다.</summary>
public static class PlatformInfrastructureExtensions
{
    /// <summary>플랫폼 DbContext와 저장소 구현을 서비스 컨테이너에 등록합니다.</summary>
    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDatabase)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDatabase);

        services.AddDbContext<PlatformDbContext>(configureDatabase);
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantMembershipRepository, TenantMembershipRepository>();
        services.AddScoped<IScopedMembershipRepository, ScopedMembershipRepository>();
        services.AddScoped<IAuthorizationPolicyRepository, AuthorizationPolicyRepository>();
        services.AddScoped<IAuthorizationAuditRepository, AuthorizationAuditRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPlatformUnitOfWork, PlatformUnitOfWork>();
        return services;
    }
}
