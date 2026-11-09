using EDAN.Tensor.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>실제 임시 SQLite API Host에서 typed Client의 Tenant·DataSet 경계를 검증합니다.</summary>
public sealed class ClientApiIntegrationTests
{
    /// <summary>typed Client가 고정 Development Bypass Tenant를 조회하고 Tenant 범위 404를 선택 조회로 변환합니다.</summary>
    [Fact]
    public async Task TypedClientUsesRealApiHostTenantBoundary()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edan-client-api-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            using var factory = new ClientApiHostFactory(root);
            using var httpClient = factory.CreateClient();
            var api = new TensorApiClient(httpClient);

            var memberships = await api.GetMyMembershipsAsync();
            var membership = Assert.Single(memberships);
            var dataSet = await api.GetDataSetAsync(membership.TenantId, Guid.NewGuid());

            Assert.Equal("Administrator", membership.Role);
            Assert.Null(dataSet);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>실제 임시 SQLite 네 데이터베이스와 Development Bypass만 사용하는 API Host를 생성합니다.</summary>
    private sealed class ClientApiHostFactory(string root) : WebApplicationFactory<Program>
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
                    ["DevelopmentData:SeedBypassTenant"] = "true",
                    ["DataProtection:DevelopmentKeyPath"] = Path.Combine(root, "keys")
                }));
            return base.CreateHost(builder);
        }
    }
}
