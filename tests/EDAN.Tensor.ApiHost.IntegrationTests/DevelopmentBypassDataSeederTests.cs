using System.Text.Json;
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

/// <summary>Development Bypass의 고정 비운영 Tenant 초기화와 중복 방지를 검증합니다.</summary>
public sealed class DevelopmentBypassDataSeederTests
{
    /// <summary>Development Bypass는 고정 비운영 Tenant와 관리자 소속을 준비하고 중복을 만들지 않습니다.</summary>
    [Fact]
    public async Task SeedsFixedNonProductionTenantAndMembershipWithoutDuplicates()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edan-tensor-bypass-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            using var factory = new BypassApiHostFactory(root);
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/api/platform/tenants/mine");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var tenant = Assert.Single(document.RootElement.EnumerateArray());
            Assert.Equal("Administrator", tenant.GetProperty("role").GetString());

            await using var scope = factory.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            Assert.Equal(1, await database.Tenants.CountAsync(x => x.Code == new TenantCode("EDAN-NONPROD")));
            Assert.Equal(1, await database.TenantMemberships.CountAsync(x =>
                x.SubjectId == "edan-nonprod-test-subject" && x.Role == TenantRole.Administrator));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>실제 Development Bypass 기본 설정과 임시 SQLite 데이터베이스만 사용하는 테스트 Host를 생성합니다.</summary>
    private sealed class BypassApiHostFactory(string root) : WebApplicationFactory<Program>
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
                    ["Authentication:Mode"] = "Bypass",
                    ["Authentication:BypassSubject"] = "untrusted-subject-override",
                    ["DevelopmentData:TenantCode"] = "UNTRUSTED-TENANT-OVERRIDE"
                });
            });
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development");
    }
}
