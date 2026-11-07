using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>Development Bypass가 사용할 로컬 Tenant와 소속을 멱등적으로 준비합니다.</summary>
internal static class DevelopmentBypassDataSeeder
{
    private const string DefaultTenantCode = "EDAN-NONPROD";
    private const string DefaultTenantName = "Edan Non-Production Development";

    /// <summary>Development Bypass가 활성화된 경우에만 로컬 Tenant와 관리자 소속을 준비합니다.</summary>
    /// <param name="services">플랫폼 데이터베이스와 로깅 서비스를 제공하는 서비스 공급자입니다.</param>
    /// <param name="configuration">인증 주체와 개발 데이터 초기화 설정을 제공하는 구성입니다.</param>
    /// <param name="cancellationToken">초기화 작업을 취소할 토큰입니다.</param>
    /// <exception cref="InvalidOperationException">개발 초기화 설정이 유효하지 않을 때 발생합니다.</exception>
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("DevelopmentData:SeedBypassTenant") ||
            !string.Equals(configuration["Authentication:Mode"], "Bypass", StringComparison.OrdinalIgnoreCase))
            return;

        var subject = configuration["Authentication:BypassSubject"]?.Trim();
        if (string.IsNullOrWhiteSpace(subject))
            throw new InvalidOperationException("Authentication:BypassSubject가 Development Bypass 초기화에 필요합니다.");

        var role = ParseRole(configuration["Authentication:BypassRole"]);
        var tenantCode = new TenantCode(configuration["DevelopmentData:TenantCode"] ?? DefaultTenantCode);
        var tenantName = configuration["DevelopmentData:TenantName"] ?? DefaultTenantName;

        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DevelopmentBypassDataSeeder");
        var profile = DevelopmentBypassExternalIdentityProfile.Fixed;
        logger.LogWarning(
            "Development Bypass가 실제 외부 IdP 대신 고정 시뮬레이션 프로필을 사용합니다. TestTenant={TestTenant}, TestUser={TestUser}, StableSubject={StableSubject}, KeyRotationPermission={KeyRotationPermission}",
            profile.TestTenant, profile.TestUser, profile.StableSubject, profile.KeyRotationPermission);

        var tenant = await database.Tenants
            .SingleOrDefaultAsync(item => item.Code == tenantCode, cancellationToken);
        var createdTenant = tenant is null;
        if (createdTenant)
        {
            tenant = Tenant.Create(new TenantId(Guid.NewGuid()), tenantCode, tenantName, DateTimeOffset.UtcNow);
            tenant.Activate();
            database.Tenants.Add(tenant);
        }

        var hasMembership = tenant is not null && await database.TenantMemberships
            .AnyAsync(item => item.TenantId == tenant.Id && item.SubjectId == subject, cancellationToken);
        var membershipCreated = false;
        if (!hasMembership && tenant is not null && tenant.Status == TenantStatus.Active)
        {
            database.TenantMemberships.Add(TenantMembership.Grant(
                new TenantMembershipId(Guid.NewGuid()), tenant.Id, subject, role));
            membershipCreated = true;
        }

        if (createdTenant || membershipCreated)
        {
            await database.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Development Bypass 로컬 Tenant 초기화를 적용했습니다. TenantCode={TenantCode}, Subject={Subject}, MembershipCreated={MembershipCreated}",
                tenantCode.Value, subject, membershipCreated);
        }
        else if (tenant is not null && tenant.Status != TenantStatus.Active)
        {
            logger.LogWarning(
                "Development Bypass 로컬 Tenant가 활성 상태가 아니므로 기존 데이터를 변경하지 않았습니다. TenantCode={TenantCode}, Status={Status}",
                tenantCode.Value, tenant.Status);
        }
    }

    /// <summary>Bypass 역할을 Tenant 역할로 변환하여 소속 권한을 결정합니다.</summary>
    /// <param name="configuredRole">인증 설정에 지정된 Bypass 역할입니다.</param>
    /// <returns>로컬 Tenant에 부여할 역할입니다.</returns>
    /// <exception cref="InvalidOperationException">지원하지 않는 역할이 지정되었을 때 발생합니다.</exception>
    private static TenantRole ParseRole(string? configuredRole)
    {
        if (string.Equals(configuredRole?.Trim(), "platform_admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(configuredRole?.Trim(), "administrator", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Administrator;

        if (string.Equals(configuredRole?.Trim(), "member", StringComparison.OrdinalIgnoreCase))
            return TenantRole.Member;

        throw new InvalidOperationException(
            "Authentication:BypassRole은 platform_admin, administrator 또는 member여야 합니다.");
    }
}
