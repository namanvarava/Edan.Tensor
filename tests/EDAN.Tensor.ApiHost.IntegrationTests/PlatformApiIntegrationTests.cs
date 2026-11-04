using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>API Host의 OpenAPI, 다중 Tenant 소속 및 데이터 기반 권한 경계를 검증합니다.</summary>
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
    public async Task TenantProvisioningUsesPersistedMembershipForTenantAuthorization()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();

        using var anonymousResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = "ANON",
            name = "익명 요청"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];

        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"IT-{suffix}",
            name = "통합 테스트 테넌트"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        using var activationResponse = await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, activationResponse.StatusCode);

        using var membershipResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/memberships", new
            {
                subjectId = "tenant-user",
                role = "Administrator"
            });
        Assert.Equal(HttpStatusCode.Created, membershipResponse.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "tenant-user");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        using var mismatchResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{Guid.NewGuid()}/organizations", new
            {
                code = "NOPE",
                name = "다른 테넌트 접근"
            });
        Assert.Equal(HttpStatusCode.Forbidden, mismatchResponse.StatusCode);

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
            organizationId,
            projectId,
            resourceType = "DataSet",
            name = "통합 테스트 데이터"
        });
        Assert.Equal(HttpStatusCode.Created, resourceResponse.StatusCode);

        using var policyResponse = await client.PostAsJsonAsync("/api/platform/policies", new
        {
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

    [Fact]
    public async Task AuthenticatedSubjectWithoutMembershipCannotAccessSelectedTenant()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "not-a-member");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", Guid.NewGuid().ToString());

        using var response = await client.PostAsJsonAsync("/api/platform/resources", new
        {
            organizationId = (Guid?)null,
            projectId = (Guid?)null,
            resourceType = "DataSet",
            name = "소속 없는 자원"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    /// <summary>각 테스트에 독립된 SQLite 데이터베이스와 테스트 전용 인증 방식을 제공합니다.</summary>
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
                    ["Database:ApplyMigrationsInDevelopment"] = "true",
                    ["Authentication:Issuer"] = "https://identity.test.edan.tensor",
                    ["Authentication:Audience"] = "edan-tensor-api",
                    ["Authentication:SigningKey"] = "integration-test-signing-key-at-least-32-bytes-long"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Integration";
                    options.DefaultChallengeScheme = "Integration";
                }).AddScheme<AuthenticationSchemeOptions, IntegrationAuthenticationHandler>("Integration", _ => { });
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
