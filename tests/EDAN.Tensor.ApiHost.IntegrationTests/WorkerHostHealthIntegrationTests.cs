extern alias workerhost;

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

using WorkerHostProgram = workerhost::Program;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>실제 임시 SQLite Worker Host의 데이터베이스 상태 경계를 검증합니다.</summary>
public sealed class WorkerHostHealthIntegrationTests
{
    /// <summary>Worker Host가 네 데이터베이스의 연결과 기본 스키마를 정상으로 보고합니다.</summary>
    [Fact]
    public async Task WorkerHostDatabaseHealthReportsAllConfiguredDatabases()
    {
        var root = Path.Combine(Path.GetTempPath(), $"edan-worker-health-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            using var factory = new WorkerHostFactory(root);
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("/health/database");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            Assert.Equal("Connected", document.RootElement.GetProperty("status").GetString());
            Assert.Equal("Green", document.RootElement.GetProperty("indicator").GetString());
            Assert.True(document.RootElement.GetProperty("canConnect").GetBoolean());
            var databases = document.RootElement.GetProperty("databases");
            Assert.True(databases.GetProperty("Platform").GetBoolean());
            Assert.True(databases.GetProperty("Worker").GetBoolean());
            Assert.True(databases.GetProperty("DataSet").GetBoolean());
            Assert.True(databases.GetProperty("DataSetContent").GetBoolean());
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private sealed class WorkerHostFactory(string root) : WebApplicationFactory<WorkerHostProgram>
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
                    ["ExternalDelivery:AllowLoopback"] = "true"
                }));
            return base.CreateHost(builder);
        }
    }
}
