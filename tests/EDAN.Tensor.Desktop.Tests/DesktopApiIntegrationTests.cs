using EDAN.Tensor.ApiHost;
using EDAN.Tensor.Desktop.Infrastructure;
using EDAN.Tensor.Desktop.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using Xunit;

namespace EDAN.Tensor.Desktop.Tests;

/// <summary>실제 임시 API Host와 WPF 화면 ViewModel의 API 호출 경계를 검증합니다.</summary>
public sealed class DesktopApiIntegrationTests
{
    /// <summary>WPF 테넌트 화면이 실제 API Host에서 인증된 개발 Tenant 소속을 조회합니다.</summary>
    [Fact]
    public async Task TenantScreenLoadsMembershipsFromRealApiHost()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edan-desktop-api-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            using var factory = new DesktopApiHostFactory(root);
            using var httpClient = factory.CreateClient();
            using var session = new DesktopApiSession(httpClient);
            await session.ConnectAsync();

            var screen = new TenantManagementViewModel(session);
            await screen.RefreshAsync();

            var target = Assert.Single(screen.TargetOptions);
            Assert.NotEqual(Guid.Empty, target.Id);
            Assert.Contains("1개의 활성 Tenant 소속", screen.StatusMessage, StringComparison.Ordinal);
            Assert.Contains(screen.Fields, field => field.Value == "Administrator");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>실제 임시 SQLite 데이터베이스와 개발 전용 인증을 사용하는 API Host를 생성합니다.</summary>
    private sealed class DesktopApiHostFactory(string root) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development");

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PlatformDatabase"] = $"Data Source={Path.Combine(root, "platform.db")};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:WorkerDatabase"] = $"Data Source={Path.Combine(root, "worker.db")};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:DataSetDatabase"] = $"Data Source={Path.Combine(root, "dataset.db")};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:DataSetContentDatabase"] = $"Data Source={Path.Combine(root, "dataset-content.db")};Cache=Shared;Pooling=False",
                    ["Database:ApplyMigrationsInDevelopment"] = "true",
                    ["AuthorizationAuditArchive:RootPath"] = Path.Combine(root, "archives"),
                    ["Authentication:Mode"] = "Bypass",
                    ["Authentication:BypassRole"] = "platform_admin",
                    ["DevelopmentData:SeedBypassTenant"] = "true",
                    ["DataProtection:DevelopmentKeyPath"] = Path.Combine(root, "keys")
                }));
            return base.CreateHost(builder);
        }
    }
}
