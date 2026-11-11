using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Services;

/// <summary>Development External 인증이 사용할 로컬 Tenant와 테스트 주체의 관리자 소속을 준비합니다.</summary>
internal static class DevelopmentExternalIdentityDataSeeder
{
    private const string DefaultTenantCode = "EDAN-NONPROD";
    private const string DefaultTenantName = "Edan Non-Production Development";

    /// <summary>명시적으로 활성화된 Development External 설정에서만 테스트 주체의 소속을 멱등적으로 생성합니다.</summary>
    /// <param name="services">플랫폼 데이터베이스와 로깅 서비스를 제공하는 서비스 공급자입니다.</param>
    /// <param name="configuration">인증 모드와 개발 테스트 주체 설정을 제공하는 구성입니다.</param>
    /// <param name="cancellationToken">초기화 작업을 취소할 토큰입니다.</param>
    /// <exception cref="InvalidOperationException">External 테스트 주체가 설정되지 않았거나 Tenant 코드가 유효하지 않을 때 발생합니다.</exception>
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("DevelopmentData:SeedExternalTenant") ||
            !string.Equals(configuration["Authentication:Mode"], "External", StringComparison.OrdinalIgnoreCase))
            return;

        var subject = configuration["DevelopmentData:ExternalTestSubject"]?.Trim();
        if (string.IsNullOrWhiteSpace(subject))
            throw new InvalidOperationException("DevelopmentData:ExternalTestSubject가 설정되지 않았습니다.");

        var tenantCodeValue = configuration["DevelopmentData:TenantCode"]?.Trim();
        if (string.IsNullOrWhiteSpace(tenantCodeValue))
            tenantCodeValue = DefaultTenantCode;

        var tenantCode = new TenantCode(tenantCodeValue);
        var tenantName = configuration["DevelopmentData:TenantName"]?.Trim();
        if (string.IsNullOrWhiteSpace(tenantName))
            tenantName = DefaultTenantName;

        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DevelopmentExternalIdentityDataSeeder");
        var testUser = configuration["DevelopmentData:ExternalTestUser"]?.Trim();

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
                new TenantMembershipId(Guid.NewGuid()), tenant.Id, subject, TenantRole.Administrator));
            membershipCreated = true;
        }

        if (createdTenant || membershipCreated)
        {
            await database.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Development External 테스트 주체의 Tenant 소속을 초기화했습니다. TenantCode={TenantCode}, TestUser={TestUser}, Subject={Subject}, MembershipCreated={MembershipCreated}",
                tenantCode.Value, testUser, subject, membershipCreated);
        }
        else if (tenant is not null && tenant.Status != TenantStatus.Active)
        {
            logger.LogWarning(
                "Development External Tenant가 활성 상태가 아니므로 기존 데이터를 변경하지 않았습니다. TenantCode={TenantCode}, Status={Status}",
                tenantCode.Value, tenant.Status);
        }
    }
}
