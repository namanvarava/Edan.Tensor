using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>API Host를 실제 HTTP 요청과 SQLite 데이터베이스로 검증합니다.</summary>
public sealed class PlatformApiIntegrationTests
{
    [Fact]
    public async Task OpenApiDocumentContainsPlatformProvisioningRoutes()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var document = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/platform/tenants", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/resources", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/policies", document, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TenantProvisioningFlowWorksThroughHttpEndpoints()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"IT-{suffix}",
            name = "통합 테스트 테넌트"
        });

        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        using var activationResponse = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, activationResponse.StatusCode);

        using var organizationResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations", new
            {
                code = "ORG",
                name = "통합 테스트 조직"
            });

        Assert.Equal(HttpStatusCode.Created, organizationResponse.StatusCode);
        var organizationId = await ReadIdAsync(organizationResponse);

        using var projectResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations/{organizationId}/projects", new
            {
                code = "CORE",
                name = "통합 테스트 프로젝트"
            });
        Assert.Equal(HttpStatusCode.Created, projectResponse.StatusCode);
        var projectId = await ReadIdAsync(projectResponse);

        using var resourceResponse = await client.PostAsJsonAsync("/api/platform/resources", new
        {
            tenantId,
            organizationId,
            projectId,
            resourceType = "DataSet",
            name = "통합 테스트 데이터"
        });
        Assert.Equal(HttpStatusCode.Created, resourceResponse.StatusCode);

        using var policyResponse = await client.PostAsJsonAsync("/api/platform/policies", new
        {
            tenantId,
            organizationId,
            projectId,
            key = "retention.days",
            value = "30"
        });
        Assert.Equal(HttpStatusCode.Created, policyResponse.StatusCode);

        using var invitationResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/users/invitations", new
            {
                email = $"user-{suffix}@example.com",
                displayName = "통합 테스트 사용자"
            });
        Assert.Equal(HttpStatusCode.Created, invitationResponse.StatusCode);
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    /// <summary>각 테스트에 독립된 개발용 SQLite 파일과 Migration 설정을 제공합니다.</summary>
    private sealed class ApiHostFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-api-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PlatformDatabase"] = $"Data Source={_databasePath};Cache=Shared",
                    ["Database:ApplyMigrationsInDevelopment"] = "true"
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(_databasePath))
                File.Delete(_databasePath);
        }
    }
}
