using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>Development External 인증의 테스트 주체 Tenant 초기화를 검증합니다.</summary>
public sealed class DevelopmentExternalIdentityDataSeederTests
{
    private const string ExternalSubject = "622f6f2a-2f18-4e7c-897e-5ee03b22ea8a";

    /// <summary>External 모드는 구성된 Keycloak 주체를 EDAN-NONPROD 관리자 소속으로 한 번만 등록합니다.</summary>
    [Fact]
    public async Task SeedsConfiguredExternalSubjectAsNonProductionAdministrator()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edan-tensor-external-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            using var factory = new ExternalApiHostFactory(root);
            using var client = factory.CreateClient();
            await using var scope = factory.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();

            Assert.Equal(1, await database.Tenants.CountAsync(item => item.Code == new TenantCode("EDAN-NONPROD")));
            Assert.Equal(1, await database.TenantMemberships.CountAsync(item =>
                item.SubjectId == ExternalSubject && item.Role == TenantRole.Administrator));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>실제 외부 IdP를 호출하지 않고 임시 SQLite로 External 초기화를 검증하는 테스트 Host를 생성합니다.</summary>
    private sealed class ExternalApiHostFactory(string root) : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PlatformDatabase"] = $"Data Source={Path.Combine(root, "platform.db")};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:WorkerDatabase"] = $"Data Source={Path.Combine(root, "worker.db")};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:DataSetDatabase"] = $"Data Source={Path.Combine(root, "dataset.db")};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:DataSetContentDatabase"] = $"Data Source={Path.Combine(root, "dataset-content.db")};Cache=Shared;Pooling=False",
                    ["Database:ApplyMigrationsInDevelopment"] = "true",
                    ["AuthorizationAuditArchive:RootPath"] = Path.Combine(root, "archives"),
                    ["Authentication:Mode"] = "External",
                    ["Authentication:Issuer"] = "http://127.0.0.1:5001/realms/EDAN-NONPROD",
                    ["Authentication:Audience"] = "edan-tensor-api",
                    ["Authentication:MetadataAddress"] = "http://127.0.0.1:5001/realms/EDAN-NONPROD/.well-known/openid-configuration",
                    ["Authentication:RequireHttpsMetadata"] = "false",
                    ["DevelopmentData:SeedExternalTenant"] = "true",
                    ["DevelopmentData:TenantCode"] = "EDAN-NONPROD",
                    ["DevelopmentData:ExternalTestUser"] = "edan-nonprod-test-user",
                    ["DevelopmentData:ExternalTestSubject"] = ExternalSubject,
                    ["DevelopmentData:TenantName"] = "Edan Non-Production Development",
                    ["DataProtection:DevelopmentKeyPath"] = Path.Combine(root, "data-protection-keys")
                });
            });
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development");
    }
}
