extern alias workerhost;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using EDAN.Tensor.DataSet.Application.Abstractions;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;
using DataSetValidationTaskHandler = workerhost::EDAN.Tensor.WorkerHost.Handlers.DataSetValidationTaskHandler;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.Platform.Infrastructure.Persistence;
using EDAN.Tensor.DataSet.Infrastructure.Persistence;
using EDAN.Tensor.DataSet.Infrastructure.Validation;
using EDAN.Tensor.HostServices.Services;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>API Host의 OpenAPI, 다중 Tenant 소속 및 데이터 기반 권한 경계를 검증합니다.</summary>
public sealed class PlatformApiIntegrationTests
{
    [Fact]
    public void FactoriesUseSeparateDatabasesBeforeServiceRegistration()
    {
        using var first = new ApiHostFactory();
        using var second = new ApiHostFactory();
        using var firstClient = first.CreateClient();
        using var secondClient = second.CreateClient();
        using var firstScope = first.Services.CreateScope();
        using var secondScope = second.Services.CreateScope();

        var firstPaths = DatabasePaths(firstScope.ServiceProvider);
        var secondPaths = DatabasePaths(secondScope.ServiceProvider);
        Assert.Equal(3, firstPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Empty(firstPaths.Intersect(secondPaths, StringComparer.OrdinalIgnoreCase));
        Assert.All(firstPaths.Concat(secondPaths), path =>
            Assert.Equal(Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetDirectoryName(Path.GetFullPath(path))));

        static string[] DatabasePaths(IServiceProvider services) =>
        [
            services.GetRequiredService<PlatformDbContext>().Database.GetDbConnection().DataSource,
            services.GetRequiredService<WorkerDbContext>().Database.GetDbConnection().DataSource,
            services.GetRequiredService<DataSetDbContext>().Database.GetDbConnection().DataSource
        ];
    }

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
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-policies", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-policies/{policyId}", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-policies/{policyId}/enabled", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/storage-quota-policies", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}/history", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}/enabled", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/integrity", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/integrity/verify", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archives", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archives/{archiveId}/verify", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/retention", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archive-schedule", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{jobId}", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{jobId}/retry", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{jobId}/verify", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/datasets", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/datasets/{dataSetId}/schemas", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/datasets/{dataSetId}/usage", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/datasets/{dataSetId}/activate", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/worker/tasks/{taskId}", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/worker/dead-letters", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/worker/dead-letters/{taskId}/replay", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships", document, StringComparison.Ordinal);
        Assert.Contains("/health", document, StringComparison.Ordinal);
        Assert.Contains("/health/database", document, StringComparison.Ordinal);
        Assert.Contains("\"Bearer\"", document, StringComparison.Ordinal);
        Assert.Contains("X-Tenant-Id", document, StringComparison.Ordinal);
        Assert.Contains("Idempotency-Key", document, StringComparison.Ordinal);
        Assert.Contains("Replay-Request-Id", document, StringComparison.Ordinal);

        using var openApiJson = JsonDocument.Parse(document);
        AssertBearerSecurityAndTenantHeader(openApiJson.RootElement,
            "/api/platform/tenants/{tenantId}/datasets/{dataSetId}", "get", requiredTenantHeader: true);
        AssertBearerSecurityAndTenantHeader(openApiJson.RootElement,
            "/api/platform/resources", "post", requiredTenantHeader: true);
        AssertBearerSecurityAndTenantHeader(openApiJson.RootElement,
            "/api/platform/tenants/mine", "get", requiredTenantHeader: false);
        AssertBearerSecurityAndTenantHeader(openApiJson.RootElement,
            "/api/platform/tenants", "post", requiredTenantHeader: false);
        AssertBearerSecurityAndTenantHeader(openApiJson.RootElement,
            "/api/platform/tenants/{tenantId}/activate", "post", requiredTenantHeader: false);
        AssertBearerSecurityAndTenantHeader(openApiJson.RootElement,
            "/api/platform/tenants/{tenantId}/memberships", "post", requiredTenantHeader: false);
        AssertRequiredHeader(openApiJson.RootElement, "/api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations", "post", "Idempotency-Key");
        AssertRequiredHeader(openApiJson.RootElement, "/api/platform/tenants/{tenantId}/worker/tasks", "post", "Idempotency-Key");
        AssertRequiredHeader(openApiJson.RootElement, "/api/platform/tenants/{tenantId}/worker/deliveries", "post", "Idempotency-Key");
        AssertRequiredHeader(openApiJson.RootElement, "/api/platform/tenants/{tenantId}/worker/dead-letters/{taskId}/replay", "post", "Replay-Request-Id");

        static void AssertRequiredHeader(JsonElement document, string path, string method, string name)
        {
            var operation = document.GetProperty("paths").GetProperty(path).GetProperty(method);
            Assert.Contains(operation.GetProperty("parameters").EnumerateArray(), parameter =>
                string.Equals(parameter.GetProperty("name").GetString(), name, StringComparison.Ordinal)
                && parameter.GetProperty("in").GetString() == "header"
                && parameter.GetProperty("required").GetBoolean());
        }

        static void AssertBearerSecurityAndTenantHeader(
            JsonElement document, string path, string method, bool requiredTenantHeader)
        {
            var operation = document.GetProperty("paths").GetProperty(path).GetProperty(method);
            Assert.Contains(operation.GetProperty("security").EnumerateArray(), requirement =>
                requirement.TryGetProperty("Bearer", out _));
            var tenantHeader = Assert.Single(operation.GetProperty("parameters").EnumerateArray(), parameter =>
                string.Equals(parameter.GetProperty("name").GetString(), "X-Tenant-Id", StringComparison.Ordinal)
                && parameter.GetProperty("in").GetString() == "header");
            var actualRequired = tenantHeader.TryGetProperty("required", out var required)
                && required.GetBoolean();
            Assert.Equal(requiredTenantHeader, actualRequired);
        }
    }

    /// <summary>Development 환경의 Swagger UI가 API OpenAPI 문서를 표시합니다.</summary>
    [Fact]
    public async Task DevelopmentSwaggerUiDisplaysOpenApiDocument()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/swagger/index.html");
        var document = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("swagger-ui", document, StringComparison.OrdinalIgnoreCase);

        using var scriptResponse = await client.GetAsync("/swagger/index.js");
        var script = await scriptResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, scriptResponse.StatusCode);
        Assert.Contains("/openapi/v1.json", script, StringComparison.Ordinal);
    }

    /// <summary>API와 데이터베이스 상태 확인 API가 모든 로컬 저장소의 상태를 반환합니다.</summary>
    [Fact]
    public async Task HealthEndpointsReportAllConfiguredDatabases()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();

        using var healthResponse = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        using var healthDocument = await JsonDocument.ParseAsync(
            await healthResponse.Content.ReadAsStreamAsync());
        Assert.Equal("Healthy", healthDocument.RootElement.GetProperty("status").GetString());
        Assert.True(healthDocument.RootElement.GetProperty("canConnect").GetBoolean());
        AssertAllDatabasesConnected(healthDocument.RootElement.GetProperty("databases"));

        using var databaseResponse = await client.GetAsync("/health/database");
        Assert.Equal(HttpStatusCode.OK, databaseResponse.StatusCode);
        using var databaseDocument = await JsonDocument.ParseAsync(
            await databaseResponse.Content.ReadAsStreamAsync());
        Assert.Equal("Connected", databaseDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal("Green", databaseDocument.RootElement.GetProperty("indicator").GetString());
        Assert.True(databaseDocument.RootElement.GetProperty("canConnect").GetBoolean());
        AssertAllDatabasesConnected(databaseDocument.RootElement.GetProperty("databases"));

        static void AssertAllDatabasesConnected(JsonElement databases)
        {
            Assert.True(databases.GetProperty("Platform").GetBoolean());
            Assert.True(databases.GetProperty("Worker").GetBoolean());
            Assert.True(databases.GetProperty("DataSet").GetBoolean());
            Assert.True(databases.GetProperty("DataSetContent").GetBoolean());
        }
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

        using var secondTenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"IT2-{suffix}",
            name = "두 번째 통합 테스트 테넌트"
        });
        Assert.Equal(HttpStatusCode.Created, secondTenantResponse.StatusCode);
        var secondTenantId = await ReadIdAsync(secondTenantResponse);
        using var secondActivationResponse = await client.PostAsync(
            $"/api/platform/tenants/{secondTenantId}/activate", content: null);
        Assert.Equal(HttpStatusCode.NoContent, secondActivationResponse.StatusCode);
        using var secondMembershipResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{secondTenantId}/memberships", new
            {
                subjectId = "tenant-user",
                role = "Member"
            });
        Assert.Equal(HttpStatusCode.Created, secondMembershipResponse.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "tenant-user");

        using var membershipsResponse = await client.GetAsync("/api/platform/tenants/mine");
        Assert.Equal(HttpStatusCode.OK, membershipsResponse.StatusCode);
        Assert.Equal(2, await ReadArrayLengthAsync(membershipsResponse));

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
        Assert.True(organizationResponse.StatusCode == HttpStatusCode.Created,
            await organizationResponse.Content.ReadAsStringAsync());
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

        // 새 구성원은 Tenant 소속만으로 조직·프로젝트 자원 생성 권한을 얻지 않습니다.
        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        using var scopedTenantMembership = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/memberships", new
            {
                subjectId = "scoped-user",
                role = "Member"
            });
        Assert.Equal(HttpStatusCode.Created, scopedTenantMembership.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "scoped-user");
        using var memberOrganization = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations", new
            {
                code = $"MEMBER-ORG-{suffix}",
                name = "구성원 조직 생성 시도"
            });
        Assert.Equal(HttpStatusCode.Forbidden, memberOrganization.StatusCode);

        using var memberProject = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations/{organizationId}/projects", new
            {
                code = $"MEMBER-PRJ-{suffix}",
                name = "구성원 프로젝트 생성 시도"
            });
        Assert.Equal(HttpStatusCode.Forbidden, memberProject.StatusCode);

        using var memberInvitation = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/users/invitations", new
            {
                email = $"member-{suffix}@example.com",
                displayName = "구성원 초대 시도"
            });
        Assert.Equal(HttpStatusCode.Forbidden, memberInvitation.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "tenant-user");
        using var organizationGrant = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships", new
            {
                subjectId = "scoped-user",
                role = "Contributor"
            });
        Assert.Equal(HttpStatusCode.Created, organizationGrant.StatusCode);
        using var projectGrant = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships", new
            {
                subjectId = "scoped-user",
                role = "Reader"
            });
        Assert.Equal(HttpStatusCode.Created, projectGrant.StatusCode);

        using var denyPolicyResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-policies", new
            {
                organizationId = (Guid?)null,
                projectId = (Guid?)null,
                subjectId = "scoped-user",
                action = "resource:create",
                resourceType = "Document",
                effect = "Deny",
                priority = 100
            });
        Assert.Equal(HttpStatusCode.Created, denyPolicyResponse.StatusCode);
        var denyPolicyId = await ReadIdAsync(denyPolicyResponse);

        using var policyListResponse = await client.GetAsync($"/api/platform/tenants/{tenantId}/authorization-policies");
        Assert.Equal(HttpStatusCode.OK, policyListResponse.StatusCode);
        Assert.True(await ReadArrayLengthAsync(policyListResponse) >= 1);

        using var policyUpdateResponse = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-policies/{denyPolicyId}",
            new { effect = "Deny", priority = 101 });
        Assert.Equal(HttpStatusCode.NoContent, policyUpdateResponse.StatusCode);

        using var policyDisableResponse = await client.PatchAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-policies/{denyPolicyId}/enabled",
            new { isEnabled = false });
        Assert.Equal(HttpStatusCode.NoContent, policyDisableResponse.StatusCode);

        using var policyEnableResponse = await client.PatchAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-policies/{denyPolicyId}/enabled",
            new { isEnabled = true });
        Assert.Equal(HttpStatusCode.NoContent, policyEnableResponse.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "scoped-user");
        using var organizationScopedResource = await client.PostAsJsonAsync("/api/platform/resources", new
        {
            organizationId,
            projectId = (Guid?)null,
            resourceType = "Document",
            name = "조직 범위 문서"
        });
        Assert.Equal(HttpStatusCode.Forbidden, organizationScopedResource.StatusCode);

        using var allowedOrganizationResource = await client.PostAsJsonAsync("/api/platform/resources", new
        {
            organizationId,
            projectId = (Guid?)null,
            resourceType = "Image",
            name = "허용된 조직 범위 이미지"
        });
        Assert.Equal(HttpStatusCode.Created, allowedOrganizationResource.StatusCode);

        using var projectScopedResource = await client.PostAsJsonAsync("/api/platform/resources", new
        {
            organizationId,
            projectId,
            resourceType = "Spreadsheet",
            name = "프로젝트 범위 문서"
        });
        Assert.Equal(HttpStatusCode.Forbidden, projectScopedResource.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "tenant-user");

        using var invitationResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/users/invitations", new
            {
                email = $"user-{suffix}@example.com",
                displayName = "통합 테스트 사용자"
            });
        Assert.Equal(HttpStatusCode.Created, invitationResponse.StatusCode);

        // Tenant 관리자는 자신의 Tenant 감사 기록을 필터와 페이지 조건으로 조회할 수 있습니다.
        using var auditResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit?page=1&pageSize=10&subjectId=tenant-user");
        Assert.Equal(HttpStatusCode.OK, auditResponse.StatusCode);
        using var auditDocument = await JsonDocument.ParseAsync(await auditResponse.Content.ReadAsStreamAsync());
        Assert.True(auditDocument.RootElement.GetProperty("totalCount").GetInt32() > 0);
        Assert.True(auditDocument.RootElement.GetProperty("items").GetArrayLength() > 0);

        using var invalidAuditPageResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit?page=1&pageSize=101");
        Assert.Equal(HttpStatusCode.BadRequest, invalidAuditPageResponse.StatusCode);

        // 기간별 체크섬을 생성하고 외부에 보관한 기준값과 다시 비교합니다.
        var fromUtc = DateTimeOffset.UtcNow.AddDays(-1);
        var toUtc = DateTimeOffset.UtcNow.AddDays(1);
        using var integrityResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/integrity?fromUtc={Uri.EscapeDataString(fromUtc.ToString("O"))}&toUtc={Uri.EscapeDataString(toUtc.ToString("O"))}");
        Assert.Equal(HttpStatusCode.OK, integrityResponse.StatusCode);
        using var integrityDocument = await JsonDocument.ParseAsync(await integrityResponse.Content.ReadAsStreamAsync());
        var digest = integrityDocument.RootElement.GetProperty("sha256").GetString();
        Assert.False(string.IsNullOrWhiteSpace(digest));

        using var verifyResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/integrity/verify", new
            {
                fromUtc,
                toUtc,
                expectedSha256 = digest
            });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        using var verifyDocument = await JsonDocument.ParseAsync(await verifyResponse.Content.ReadAsStreamAsync());
        Assert.True(verifyDocument.RootElement.GetProperty("isValid").GetBoolean());
        // 아카이브 작업 생성 후 상태와 실행 이력을 조회하고 체크섬을 검증합니다.
        using var archiveJobResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs", new
            {
                fromUtc,
                toUtc
            });
        Assert.Equal(HttpStatusCode.Created, archiveJobResponse.StatusCode);
        using var archiveJobDocument = await JsonDocument.ParseAsync(await archiveJobResponse.Content.ReadAsStreamAsync());
        var archiveJobId = archiveJobDocument.RootElement.GetProperty("jobId").GetGuid();
        Assert.Equal("Completed", archiveJobDocument.RootElement.GetProperty("status").GetString());

        using var archiveJobDetailsResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{archiveJobId}");
        Assert.Equal(HttpStatusCode.OK, archiveJobDetailsResponse.StatusCode);
        using var archiveJobDetails = await JsonDocument.ParseAsync(await archiveJobDetailsResponse.Content.ReadAsStreamAsync());
        Assert.Single(archiveJobDetails.RootElement.GetProperty("attempts").EnumerateArray());

        using var archiveJobVerifyResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/archive-jobs/{archiveJobId}/verify",
            new { expectedSha256 = (string?)null });
        Assert.Equal(HttpStatusCode.OK, archiveJobVerifyResponse.StatusCode);
        using var archiveJobVerify = await JsonDocument.ParseAsync(await archiveJobVerifyResponse.Content.ReadAsStreamAsync());
        Assert.True(archiveJobVerify.RootElement.GetProperty("isValid").GetBoolean());

        // Tenant별 예약 정책을 조회하고 실행 주기와 활성 상태를 변경합니다.
        using var scheduleGetResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/archive-schedule");
        Assert.Equal(HttpStatusCode.OK, scheduleGetResponse.StatusCode);
        using var scheduleDefault = await JsonDocument.ParseAsync(await scheduleGetResponse.Content.ReadAsStreamAsync());
        Assert.False(scheduleDefault.RootElement.GetProperty("isEnabled").GetBoolean());

        using var scheduleUpdateResponse = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/archive-schedule",
            new { intervalHours = 24, isEnabled = true });
        Assert.Equal(HttpStatusCode.OK, scheduleUpdateResponse.StatusCode);
        using var scheduleUpdated = await JsonDocument.ParseAsync(await scheduleUpdateResponse.Content.ReadAsStreamAsync());
        Assert.True(scheduleUpdated.RootElement.GetProperty("isEnabled").GetBoolean());
        Assert.Equal(24, scheduleUpdated.RootElement.GetProperty("intervalHours").GetInt32());

        using var invalidScheduleResponse = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/archive-schedule",
            new { intervalHours = 800, isEnabled = true });
        Assert.Equal(HttpStatusCode.BadRequest, invalidScheduleResponse.StatusCode);

        // Tenant별 감사 보존 정책을 조회하고 법적 보존 상태를 변경합니다.
        using var retentionGetResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/retention");
        Assert.Equal(HttpStatusCode.OK, retentionGetResponse.StatusCode);
        using var retentionDefault = await JsonDocument.ParseAsync(await retentionGetResponse.Content.ReadAsStreamAsync());
        Assert.Equal(365, retentionDefault.RootElement.GetProperty("retentionDays").GetInt32());

        using var retentionUpdateResponse = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/retention", new
            {
                retentionDays = 730,
                isLegalHold = true,
                legalHoldReason = "보안 사고 조사 보존"
            });
        Assert.Equal(HttpStatusCode.OK, retentionUpdateResponse.StatusCode);
        using var retentionUpdated = await JsonDocument.ParseAsync(await retentionUpdateResponse.Content.ReadAsStreamAsync());
        Assert.True(retentionUpdated.RootElement.GetProperty("isLegalHold").GetBoolean());
        Assert.Equal(730, retentionUpdated.RootElement.GetProperty("retentionDays").GetInt32());

        using var invalidRetentionResponse = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/authorization-audit/retention", new
            {
                retentionDays = 10,
                isLegalHold = false,
                legalHoldReason = (string?)null
            });
        Assert.Equal(HttpStatusCode.BadRequest, invalidRetentionResponse.StatusCode);

    }

    [Fact]
    public async Task DataSetManagementApiEnforcesTenantScopeAndPersistsMetadata()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();

        using var anonymous = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{Guid.NewGuid()}/datasets",
            new { organizationId = Guid.NewGuid(), projectId = Guid.NewGuid(), name = "anonymous" });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"DS-{suffix}",
            name = "DataSet API 테스트 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        using var activation = await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activation.StatusCode);
        using var membership = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/memberships",
            new { subjectId = "dataset-admin", role = "Administrator" });
        Assert.Equal(HttpStatusCode.Created, membership.StatusCode);
        using var memberMembership = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/memberships",
            new { subjectId = "dataset-member", role = "Member" });
        Assert.Equal(HttpStatusCode.Created, memberMembership.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "dataset-admin");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        using var organization = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations",
            new { code = $"ORG-{suffix}", name = "DataSet 조직" });
        Assert.Equal(HttpStatusCode.Created, organization.StatusCode);
        var organizationId = await ReadIdAsync(organization);

        using var project = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/organizations/{organizationId}/projects",
            new { code = $"PRJ-{suffix}", name = "DataSet 프로젝트" });
        Assert.Equal(HttpStatusCode.Created, project.StatusCode);
        var projectId = await ReadIdAsync(project);

        using var invalidScope = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets",
            new { organizationId = Guid.NewGuid(), projectId, name = "잘못된 소유 범위" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidScope.StatusCode);

        using var invalidAdapter = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets",
            new { organizationId, projectId, name = "잘못된 저장소 어댑터", storageAdapter = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, invalidAdapter.StatusCode);
        using var invalidAdapterDocument = await JsonDocument.ParseAsync(
            await invalidAdapter.Content.ReadAsStreamAsync());
        Assert.Equal("dataset.validation", invalidAdapterDocument.RootElement.GetProperty("code").GetString());

        using var unavailableAdapter = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets",
            new { organizationId, projectId, name = "구현되지 않은 저장소 어댑터", storageAdapter = "duckdb" });
        Assert.Equal(HttpStatusCode.BadRequest, unavailableAdapter.StatusCode);
        using var unavailableAdapterDocument = await JsonDocument.ParseAsync(
            await unavailableAdapter.Content.ReadAsStreamAsync());
        Assert.Equal("dataset.adapter_unavailable", unavailableAdapterDocument.RootElement.GetProperty("code").GetString());

        await using (var contentScope = factory.Services.CreateAsyncScope())
        {
            var contentUsage = contentScope.ServiceProvider
                .GetRequiredService<IDataSetTenantStorageUsageReader>();
            var usage = await contentUsage.GetTenantUsageAsync(tenantId);
            Assert.Equal(0, usage.TenantBytes);
            Assert.Equal(0, usage.TenantDataSetCount);
        }

        using var create = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets",
            new { organizationId, projectId, name = "고객 원천 데이터", storageAdapter = "sqlite" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createdDocument = await JsonDocument.ParseAsync(await create.Content.ReadAsStreamAsync());
        var dataSetId = createdDocument.RootElement.GetProperty("id").GetGuid();
        Assert.Equal("Draft", createdDocument.RootElement.GetProperty("status").GetString());

        using var emptyActivation = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/activate", null);
        Assert.Equal(HttpStatusCode.Conflict, emptyActivation.StatusCode);

        using var addSchema = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/schemas",
            new { schemaJson = "{\"type\":\"object\",\"properties\":{\"id\":{\"type\":\"string\"}}}" });
        Assert.True(addSchema.StatusCode == HttpStatusCode.Created,
            await addSchema.Content.ReadAsStringAsync());

        using var activationAfterSchema = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activationAfterSchema.StatusCode);

        const string payload = "EDAN Tensor DataSet content round-trip";
        using var uploadContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(payload));
        uploadContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        using var upload = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content", uploadContent);
        Assert.Equal(HttpStatusCode.NoContent, upload.StatusCode);

        using var download = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content");
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(payload, await download.Content.ReadAsStringAsync());

        using var usageResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/usage");
        Assert.Equal(HttpStatusCode.OK, usageResponse.StatusCode);
        using var usageDocument = await JsonDocument.ParseAsync(await usageResponse.Content.ReadAsStreamAsync());
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(payload),
            usageDocument.RootElement.GetProperty("tenantBytes").GetInt64());
        Assert.Equal(1, usageDocument.RootElement.GetProperty("tenantDataSetCount").GetInt64());
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(payload),
            usageDocument.RootElement.GetProperty("organizationBytes").GetInt64());
        Assert.Equal(1, usageDocument.RootElement.GetProperty("organizationDataSetCount").GetInt64());
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(payload),
            usageDocument.RootElement.GetProperty("projectBytes").GetInt64());
        Assert.Equal(1, usageDocument.RootElement.GetProperty("projectDataSetCount").GetInt64());
        Assert.Equal(System.Text.Encoding.UTF8.GetByteCount(payload),
            usageDocument.RootElement.GetProperty("dataSetBytes").GetInt64());
        Assert.Equal(50, usageDocument.RootElement.GetProperty("limits").GetProperty("tenantLimitBytes").GetInt64());

        using var quotaExceededContent = new ByteArrayContent(new byte[51]);
        using var quotaExceededUpload = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content", quotaExceededContent);
        Assert.Equal(HttpStatusCode.Conflict, quotaExceededUpload.StatusCode);
        using var quotaExceededDocument = await JsonDocument.ParseAsync(await quotaExceededUpload.Content.ReadAsStreamAsync());
        Assert.Equal("dataset.quota_exceeded", quotaExceededDocument.RootElement.GetProperty("code").GetString());

        using var oversizedContent = new ByteArrayContent(new byte[1025]);
        using var oversizedUpload = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content", oversizedContent);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, oversizedUpload.StatusCode);
        using var get = await client.GetAsync($"/api/platform/tenants/{tenantId}/datasets/{dataSetId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        using var dataSetDocument = await JsonDocument.ParseAsync(await get.Content.ReadAsStreamAsync());
        var dataSet = dataSetDocument.RootElement;
        Assert.Equal(tenantId, dataSet.GetProperty("tenantId").GetGuid());
        Assert.Equal(organizationId, dataSet.GetProperty("organizationId").GetGuid());
        Assert.Equal(projectId, dataSet.GetProperty("projectId").GetGuid());
        Assert.Equal("Active", dataSet.GetProperty("status").GetString());
        Assert.Equal(1, dataSet.GetProperty("currentSchemaVersion").GetInt32());
        Assert.Single(dataSet.GetProperty("schemaRevisions").EnumerateArray());
        Assert.False(dataSet.TryGetProperty("storageReference", out _));

        using var otherTenantAccess = await client.GetAsync(
            $"/api/platform/tenants/{Guid.NewGuid()}/datasets/{dataSetId}");
        Assert.Equal(HttpStatusCode.Forbidden, otherTenantAccess.StatusCode);


        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "dataset-member");
        using var memberCreate = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets",
            new { organizationId, projectId, name = "권한 없는 DataSet 생성" });
        Assert.Equal(HttpStatusCode.Forbidden, memberCreate.StatusCode);        using var memberRead = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content");
        Assert.Equal(HttpStatusCode.OK, memberRead.StatusCode);
        using var memberUploadContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("unauthorized"));
        using var memberUpload = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content", memberUploadContent);
        Assert.Equal(HttpStatusCode.Forbidden, memberUpload.StatusCode);
    }

    [Fact]
    public async Task DataSetValidationApiCreatesProfileAndIdempotentWorkerExecution()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];

        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"DV-{suffix}", name = "DataSet 검증 Tenant"
        });
        var tenantId = await ReadIdAsync(tenantResponse);
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync($"/api/platform/tenants/{tenantId}/memberships",
                new { subjectId = "validation-admin", role = "Administrator" })).StatusCode);

        using var scope = factory.Services.CreateScope();
        var workerDb = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        var validationTaskType = new WorkerTaskTypeCatalogEntry(
            DataSetValidationTaskContract.TaskType, "DataSet 검증 v1", DateTimeOffset.UtcNow);
        workerDb.WorkerTaskTypes.Add(validationTaskType);
        await workerDb.SaveChangesAsync();

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "validation-admin");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // 플랫폼 소유 범위는 테스트 데이터베이스에 직접 구성하여 검증 API 경계만 확인합니다.
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var organization = Organization.Create(new OrganizationId(organizationId), new TenantId(tenantId), new OrganizationCode($"ORG-{suffix}"), "검증 조직");
        platformDb.Organizations.Add(organization);
        var project = Project.Create(new ProjectId(projectId), new TenantId(tenantId), organization.Id, new ProjectCode($"PRJ-{suffix}"), "검증 프로젝트");
        platformDb.Projects.Add(project);
        await platformDb.SaveChangesAsync();

        using var dataSetResponse = await client.PostAsJsonAsync($"/api/platform/tenants/{tenantId}/datasets",
            new { organizationId, projectId, name = "검증 원천", contentFormat = "csv" });
        Assert.Equal(HttpStatusCode.Created, dataSetResponse.StatusCode);
        using var dataSetJson = await JsonDocument.ParseAsync(await dataSetResponse.Content.ReadAsStreamAsync());
        var dataSetId = dataSetJson.RootElement.GetProperty("id").GetGuid();

        Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/schemas",
            new { schemaJson = "{\"type\":\"object\"}" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync($"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/activate", null)).StatusCode);
        using var content = new StringContent("id,name\n1,alpha\n1,beta\n", Encoding.UTF8, "text/csv");
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync($"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/content", content)).StatusCode);

        using var profileResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/datasets/validation-profiles",
            new { name = "기본 검증", definitionJson = "{\"fields\":[{\"name\":\"id\",\"type\":\"integer\",\"required\":true}],\"duplicateSets\":[{\"fields\":[\"id\"]}]}" });
        Assert.Equal(HttpStatusCode.Created, profileResponse.StatusCode);
        using var profileJson = await JsonDocument.ParseAsync(await profileResponse.Content.ReadAsStreamAsync());
        var profileId = profileJson.RootElement.GetProperty("id").GetGuid();

        using var execute = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations")
        {
            Content = JsonContent.Create(new { expectedDataSetVersion = 3, schemaVersion = 1, profileId, profileRevision = 1 })
        };
        execute.Headers.Add("Idempotency-Key", "validation-integration-1");
        using var executionResponse = await client.SendAsync(execute);
        Assert.Equal(HttpStatusCode.Accepted, executionResponse.StatusCode);
        using var executionJson = await JsonDocument.ParseAsync(await executionResponse.Content.ReadAsStreamAsync());
        var executionId = executionJson.RootElement.GetProperty("execution").GetProperty("id").GetGuid();
        var workerTaskId = executionJson.RootElement.GetProperty("workerTaskId").GetGuid();

        using var duplicate = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations")
        {
            Content = JsonContent.Create(new { expectedDataSetVersion = 3, schemaVersion = 1, profileId, profileRevision = 1 })
        };
        duplicate.Headers.Add("Idempotency-Key", "validation-integration-1");
        using var duplicateResponse = await client.SendAsync(duplicate);
        Assert.Equal(HttpStatusCode.Accepted, duplicateResponse.StatusCode);
        using var duplicateJson = await JsonDocument.ParseAsync(await duplicateResponse.Content.ReadAsStreamAsync());
        Assert.Equal(executionId, duplicateJson.RootElement.GetProperty("execution").GetProperty("id").GetGuid());
        Assert.False(duplicateJson.RootElement.GetProperty("created").GetBoolean());

        using var conflicting = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations")
        {
            Content = JsonContent.Create(new { expectedDataSetVersion = 2, schemaVersion = 1, profileId, profileRevision = 1 })
        };
        conflicting.Headers.Add("Idempotency-Key", "validation-integration-1");
        using var conflictingResponse = await client.SendAsync(conflicting);
        Assert.Equal(HttpStatusCode.Conflict, conflictingResponse.StatusCode);

        await using var validationScope = factory.Services.CreateAsyncScope();
        var dataSetDb = validationScope.ServiceProvider.GetRequiredService<DataSetDbContext>();
        Assert.Equal(1, await dataSetDb.ValidationExecutions.CountAsync(
            execution => execution.TenantId == tenantId
                && execution.IdempotencyKey == "validation-integration-1"));

        using var status = await client.GetAsync($"/api/platform/tenants/{tenantId}/datasets/validations/{executionId}");
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        using var statusJson = await JsonDocument.ParseAsync(await status.Content.ReadAsStreamAsync());
        Assert.Equal("Queued", statusJson.RootElement.GetProperty("status").GetString());

        var handler = new DataSetValidationTaskHandler(
            factory.Services.GetRequiredService<IServiceScopeFactory>());
        var coordinator = new WorkerTaskExecutionCoordinator(
            new WorkerTaskRepository(workerDb),
            new WorkerTaskHandlerRegistry([
                new WorkerTaskHandlerRegistration(DataSetValidationTaskContract.TaskType, handler)
            ]),
            new SuccessfulLeaseRenewer(),
            new WorkerExecutionRegistry(),
            TimeProvider.System);

        var workerOutcome = await coordinator.ExecuteAsync(
            tenantId, workerTaskId, "integration-worker", TimeSpan.FromMinutes(5));
        Assert.Equal(WorkerTaskExecutionOutcome.Succeeded, workerOutcome);

        await using var completedScope = factory.Services.CreateAsyncScope();
        var completedWorkerDb = completedScope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        var completedTask = await completedWorkerDb.WorkerTasks.SingleAsync(task => task.Id == workerTaskId);
        Assert.Equal(WorkerTaskStatus.Succeeded, completedTask.Status);
        var completedDataSetDb = completedScope.ServiceProvider.GetRequiredService<DataSetDbContext>();
        var completedExecution = await completedDataSetDb.ValidationExecutions
            .SingleAsync(item => item.Id == executionId);
        Assert.Equal("Invalid", completedExecution.Status);
        Assert.True(completedExecution.ErrorCount > 0);
        Assert.Equal(2, completedExecution.ProcessedRecords);

        using var details = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/datasets/validations/{executionId}/details");
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        using var detailsJson = await JsonDocument.ParseAsync(await details.Content.ReadAsStreamAsync());
        var detailItems = detailsJson.RootElement.GetProperty("items").EnumerateArray().ToArray();
        Assert.NotEmpty(detailItems);
        Assert.All(detailItems, item =>
        {
            var message = item.GetProperty("message").GetString();
            Assert.DoesNotContain("alpha", message, StringComparison.Ordinal);
            Assert.DoesNotContain("beta", message, StringComparison.Ordinal);
        });

        using var delete = await client.DeleteAsync(
            $"/api/platform/tenants/{tenantId}/datasets/validations/{executionId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        using var deleted = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/datasets/validations/{executionId}");
        Assert.Equal(HttpStatusCode.NotFound, deleted.StatusCode);

        workerDb.WorkerTaskTypes.Remove(validationTaskType);
        await workerDb.SaveChangesAsync();
        using var unavailable = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/datasets/{dataSetId}/validations")
        {
            Content = JsonContent.Create(new
            {
                expectedDataSetVersion = 3, schemaVersion = 1, profileId, profileRevision = 1
            })
        };
        unavailable.Headers.Add("Idempotency-Key", "validation-type-unavailable-1");
        using var unavailableResponse = await client.SendAsync(unavailable);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, unavailableResponse.StatusCode);
        await using var unavailableScope = factory.Services.CreateAsyncScope();
        var unavailableDb = unavailableScope.ServiceProvider.GetRequiredService<DataSetDbContext>();
        Assert.Equal(0, await unavailableDb.ValidationExecutions.CountAsync(
            execution => execution.TenantId == tenantId
                && execution.IdempotencyKey == "validation-type-unavailable-1"));
    }

    [Fact]
    public async Task ExternalDeliveryApiUsesOpaqueReferenceAndIdempotencyBoundary()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"ED-{suffix}", name = "외부 전달 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync($"/api/platform/tenants/{tenantId}/memberships",
                new { subjectId = "delivery-admin", role = "Administrator" })).StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var workerDb = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        workerDb.WorkerTaskTypes.Add(new WorkerTaskTypeCatalogEntry(
            EDAN.Tensor.Worker.Application.ExternalDelivery.ExternalDeliveryTaskContract.TaskType,
            "외부 전달 v1", DateTimeOffset.UtcNow));
        await workerDb.SaveChangesAsync();

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "delivery-admin");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/worker/deliveries")
        {
            Content = JsonContent.Create(new
            {
                targetUri = "http://127.0.0.1:4567/receiver",
                payload = new { secret = "must-not-be-returned" },
                maxAttempts = 3
            })
        };
        request.Headers.Add("Idempotency-Key", "delivery-api-1");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var responseText = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("must-not-be-returned", responseText, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(responseText);
        var deliveryId = document.RootElement.GetProperty("delivery").GetProperty("id").GetGuid();

        using var duplicate = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/worker/deliveries")
        {
            Content = JsonContent.Create(new
            {
                targetUri = "http://127.0.0.1:4567/receiver",
                payload = new { secret = "must-not-be-returned" },
                maxAttempts = 3
            })
        };
        duplicate.Headers.Add("Idempotency-Key", "delivery-api-1");
        using var duplicateResponse = await client.SendAsync(duplicate);
        Assert.Equal(HttpStatusCode.Accepted, duplicateResponse.StatusCode);
        using var duplicateDocument = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        Assert.Equal(deliveryId, duplicateDocument.RootElement.GetProperty("delivery").GetProperty("id").GetGuid());

        using var status = await client.GetAsync($"/api/platform/tenants/{tenantId}/worker/deliveries/{deliveryId}");
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
    }

    [Fact]
    public async Task ExternalDeliveryWorkerConflictRemovesOnlyNewPendingDelivery()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"EDC-{suffix}", name = "외부 전달 충돌 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync($"/api/platform/tenants/{tenantId}/memberships",
                new { subjectId = "delivery-conflict-admin", role = "Administrator" })).StatusCode);

        const string idempotencyKey = "delivery-worker-conflict-1";
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var workerDb = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
            workerDb.WorkerTaskTypes.Add(new WorkerTaskTypeCatalogEntry(
                EDAN.Tensor.Worker.Application.ExternalDelivery.ExternalDeliveryTaskContract.TaskType,
                "외부 전달 v1", DateTimeOffset.UtcNow));
            workerDb.WorkerTasks.Add(WorkerTask.Create(tenantId, "integration.other", idempotencyKey,
                "private://conflicting-task", DateTimeOffset.UtcNow, maxAttempts: 3));
            await workerDb.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "delivery-conflict-admin");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/worker/deliveries")
        {
            Content = JsonContent.Create(new
            {
                targetUri = "http://127.0.0.1:4567/receiver",
                payload = new { eventName = "conflict" },
                maxAttempts = 3
            })
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        Assert.Equal(0, await verificationDb.ExternalDeliveries.CountAsync(
            delivery => delivery.TenantId == tenantId && delivery.IdempotencyKey == idempotencyKey));
        Assert.Equal(1, await verificationDb.WorkerTasks.CountAsync(
            task => task.TenantId == tenantId && task.IdempotencyKey == idempotencyKey));
    }

    [Fact]
    public async Task ExternalDeliveryUnavailableWorkerTypeRemovesNewPendingDelivery()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"EDU-{suffix}", name = "외부 전달 유형 미사용 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);
        Assert.Equal(HttpStatusCode.NoContent,
            (await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,
            (await client.PostAsJsonAsync($"/api/platform/tenants/{tenantId}/memberships",
                new { subjectId = "delivery-unavailable-admin", role = "Administrator" })).StatusCode);

        const string idempotencyKey = "delivery-worker-type-unavailable-1";
        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "delivery-unavailable-admin");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"/api/platform/tenants/{tenantId}/worker/deliveries")
        {
            Content = JsonContent.Create(new
            {
                targetUri = "http://127.0.0.1:4567/receiver",
                payload = new { eventName = "unavailable" },
                maxAttempts = 3
            })
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<WorkerDbContext>();
        Assert.Equal(0, await verificationDb.ExternalDeliveries.CountAsync(
            delivery => delivery.TenantId == tenantId && delivery.IdempotencyKey == idempotencyKey));
        Assert.Equal(0, await verificationDb.WorkerTasks.CountAsync(
            task => task.TenantId == tenantId && task.IdempotencyKey == idempotencyKey));
    }

    [Fact]
    public async Task WorkerDeadLetterOperationsRequireTenantAdministratorAndPreserveOriginalTask()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        var tenantId = Guid.NewGuid();

        using var anonymous = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"DLQ-{suffix}",
            name = "DLQ 통합 테스트 테넌트"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        tenantId = await ReadIdAsync(tenantResponse);
        using var activation = await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activation.StatusCode);

        using var adminMembership = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/memberships",
            new { subjectId = "worker-admin", role = "Administrator" });
        Assert.Equal(HttpStatusCode.Created, adminMembership.StatusCode);
        using var memberMembership = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/memberships",
            new { subjectId = "worker-member", role = "Member" });
        Assert.Equal(HttpStatusCode.Created, memberMembership.StatusCode);

        var now = DateTimeOffset.UtcNow;
        var task = WorkerTask.Create(tenantId, "integration.test", $"dlq:{Guid.NewGuid():N}",
            "private://payload/should-not-be-exposed", now, maxAttempts: 1);
        var leaseId = Guid.NewGuid();
        task.AcquireLease("integration-worker", leaseId, now, now.AddMinutes(5));
        task.Start(leaseId, task.FencingToken, now.AddSeconds(1));
        task.Fail(leaseId, task.FencingToken, "integration.failure",
            now.AddSeconds(2), now.AddMinutes(1));
        using (var scope = factory.Services.CreateScope())
        {
            var workerDb = scope.ServiceProvider.GetRequiredService<WorkerDbContext>();
            workerDb.WorkerTasks.Add(task);
            await workerDb.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "worker-member");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
        using var memberDenied = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters");
        Assert.Equal(HttpStatusCode.Forbidden, memberDenied.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "worker-admin");
        using var missingIdempotencyKey = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks",
            new { taskType = "integration.enqueue", payloadReference = "private://payload/input", maxAttempts = 3 });
        Assert.Equal(HttpStatusCode.BadRequest, missingIdempotencyKey.StatusCode);
        Assert.Equal("worker.enqueue.idempotency_key_required", await ReadErrorCodeAsync(missingIdempotencyKey));

        using var missingReplayRequestId = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters/{Guid.NewGuid()}/replay", null);
        Assert.Equal(HttpStatusCode.BadRequest, missingReplayRequestId.StatusCode);
        Assert.Equal("worker.dead_letter.replay_request_id_required", await ReadErrorCodeAsync(missingReplayRequestId));

        // WorkerHost가 등록하지 않은 작업 유형은 작업 레코드를 만들지 않고 거부합니다.
        client.DefaultRequestHeaders.Add("Idempotency-Key", $"unknown:{Guid.NewGuid():N}");
        using var unknownTaskType = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks",
            new { taskType = "integration.unknown", payloadReference = "private://payload/input", maxAttempts = 3 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknownTaskType.StatusCode);
        client.DefaultRequestHeaders.Remove("Idempotency-Key");

        // 테스트 WorkerHost가 등록한 유형을 카탈로그에 준비합니다.
        using (var catalogScope = factory.Services.CreateScope())
        {
            var workerDb = catalogScope.ServiceProvider.GetRequiredService<WorkerDbContext>();
            workerDb.WorkerTaskTypes.Add(new WorkerTaskTypeCatalogEntry(
                "integration.enqueue", "통합 테스트 작업 유형", DateTimeOffset.UtcNow));
            await workerDb.SaveChangesAsync();
        }

        // 동일 Idempotency Key 재요청은 기존 작업을 반환하고, 다른 내용은 충돌로 거부합니다.
        var idempotencyKey = $"api-enqueue:{Guid.NewGuid():N}";
        client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);
        using var enqueueResponse = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks",
            new { taskType = "integration.enqueue", payloadReference = "private://payload/input", maxAttempts = 3 });
        Assert.Equal(HttpStatusCode.Created, enqueueResponse.StatusCode);
        using var enqueueDocument = await JsonDocument.ParseAsync(await enqueueResponse.Content.ReadAsStreamAsync());
        var enqueuedTaskId = enqueueDocument.RootElement.GetProperty("id").GetGuid();
        Assert.False(enqueueDocument.RootElement.TryGetProperty("payloadReference", out _));

        using var taskStatusResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks/{enqueuedTaskId}");
        Assert.Equal(HttpStatusCode.OK, taskStatusResponse.StatusCode);
        using var taskStatusDocument = await JsonDocument.ParseAsync(await taskStatusResponse.Content.ReadAsStreamAsync());
        var taskStatus = taskStatusDocument.RootElement;
        Assert.Equal(enqueuedTaskId, taskStatus.GetProperty("id").GetGuid());
        Assert.Equal("Pending", taskStatus.GetProperty("status").GetString());
        Assert.Equal(0, taskStatus.GetProperty("attemptCount").GetInt32());
        Assert.False(taskStatus.TryGetProperty("payloadReference", out _));
        Assert.False(taskStatus.TryGetProperty("checkpoint", out _));
        Assert.Equal(0, taskStatus.GetProperty("progressPercent").GetInt32());
        Assert.False(taskStatus.GetProperty("hasResult").GetBoolean());

        using var missingTaskStatus = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missingTaskStatus.StatusCode);

        // 다른 Tenant는 현재 Tenant 선택 및 멤버십 경계에서 차단됩니다.
        using var crossTenantTaskStatus = await client.GetAsync(
            $"/api/platform/tenants/{Guid.NewGuid()}/worker/tasks/{enqueuedTaskId}");
        Assert.Equal(HttpStatusCode.Forbidden, crossTenantTaskStatus.StatusCode);

        using var duplicateEnqueue = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks",
            new { taskType = "integration.enqueue", payloadReference = "private://payload/input", maxAttempts = 3 });
        Assert.Equal(HttpStatusCode.OK, duplicateEnqueue.StatusCode);
        using var duplicateDocument = await JsonDocument.ParseAsync(await duplicateEnqueue.Content.ReadAsStreamAsync());
        Assert.Equal(enqueuedTaskId, duplicateDocument.RootElement.GetProperty("id").GetGuid());

        using var conflictingEnqueue = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/worker/tasks",
            new { taskType = "integration.enqueue", payloadReference = "private://payload/changed", maxAttempts = 3 });
        Assert.Equal(HttpStatusCode.Conflict, conflictingEnqueue.StatusCode);
        client.DefaultRequestHeaders.Remove("Idempotency-Key");

        using var listResponse = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listDocument = await JsonDocument.ParseAsync(await listResponse.Content.ReadAsStreamAsync());
        var items = listDocument.RootElement.GetProperty("items");
        Assert.Single(items.EnumerateArray());
        Assert.Equal(task.Id, items[0].GetProperty("id").GetGuid());
        Assert.False(items[0].TryGetProperty("payloadReference", out _));

        var replayRequestId = Guid.NewGuid();
        client.DefaultRequestHeaders.Add("Replay-Request-Id", replayRequestId.ToString());
        using var replayResponse = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters/{task.Id}/replay", null);
        Assert.Equal(HttpStatusCode.Created, replayResponse.StatusCode);
        using var replayDocument = await JsonDocument.ParseAsync(await replayResponse.Content.ReadAsStreamAsync());
        var replayTaskId = replayDocument.RootElement.GetProperty("replayTaskId").GetGuid();
        Assert.NotEqual(task.Id, replayTaskId);

        // 동일 요청 ID의 재전송은 새 작업을 만들지 않고 기존 재처리 작업을 반환합니다.
        using var duplicateReplayResponse = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters/{task.Id}/replay", null);
        Assert.Equal(HttpStatusCode.Created, duplicateReplayResponse.StatusCode);
        using var duplicateReplayDocument = await JsonDocument.ParseAsync(
            await duplicateReplayResponse.Content.ReadAsStreamAsync());
        Assert.Equal(replayTaskId, duplicateReplayDocument.RootElement.GetProperty("replayTaskId").GetGuid());
        client.DefaultRequestHeaders.Remove("Replay-Request-Id");

        using var repeatedList = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters");
        Assert.Equal(HttpStatusCode.OK, repeatedList.StatusCode);
        using var repeatedDocument = await JsonDocument.ParseAsync(await repeatedList.Content.ReadAsStreamAsync());
        Assert.Equal(task.Id, repeatedDocument.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid());
    }


    [Fact]
    public async Task QuotaThresholdEventsRecordWarningExceededAndResolvedTransitionsOnlyOnce()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"QT-{suffix}",
            name = "Quota 전이 테스트 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        await using var scope = factory.Services.CreateAsyncScope();
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var dataSetDb = scope.ServiceProvider.GetRequiredService<DataSetDbContext>();
        var storageAdapter = scope.ServiceProvider.GetRequiredService<IDataSetStorageAdapter>();
        var snapshotService = scope.ServiceProvider.GetRequiredService<DataSetStorageUsageSnapshotService>();

        var allocation = new DataSetStorageAllocation(
            new EDAN.Tensor.DataSet.Domain.Model.DataSetId(Guid.NewGuid()),
            new EDAN.Tensor.DataSet.Domain.Model.DataSetScope(tenantId, Guid.NewGuid(), Guid.NewGuid()));
        var storageReference = await storageAdapter.AllocateAsync(allocation);
        await using (var content = new MemoryStream(new byte[45]))
            await storageAdapter.WriteAsync(allocation, storageReference, content,
                new DataSetStorageQuotaPolicy(null, null, null, null));

        var policy = StorageQuotaPolicy.Create(
            Guid.NewGuid(), new TenantId(tenantId), StorageQuotaPolicyScope.Tenant, tenantId,
            tenantLimitBytes: 50, organizationLimitBytes: null, projectLimitBytes: null, dataSetLimitBytes: null,
            warningThresholdPercent: 80, planCode: "Community", updatedBy: "integration-test",
            nowUtc: DateTimeOffset.UtcNow);
        platformDb.StorageQuotaPolicies.Add(policy);
        await platformDb.SaveChangesAsync();

        await snapshotService.CaptureDueAsync();
        await snapshotService.CaptureDueAsync();
        var warningOnly = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.PolicyId == policy.Id).OrderBy(x => x.Id).ToListAsync();
        var warning = Assert.Single(warningOnly);
        Assert.Equal("Warning", warning.EventType);
        Assert.Equal(45, warning.UsedBytes);
        Assert.Equal(50, warning.LimitBytes);
        Assert.Equal(1, warning.PolicyVersion);

        policy.Update(tenantLimitBytes: 40, organizationLimitBytes: null, projectLimitBytes: null,
            dataSetLimitBytes: null, warningThresholdPercent: 80, planCode: "Community",
            updatedBy: "integration-test", expectedVersion: policy.Version, nowUtc: DateTimeOffset.UtcNow);
        await platformDb.SaveChangesAsync();
        await snapshotService.CaptureDueAsync();

        policy.SetEnabled(false, "integration-test", policy.Version, DateTimeOffset.UtcNow);
        await platformDb.SaveChangesAsync();
        await snapshotService.CaptureDueAsync();
        await snapshotService.CaptureDueAsync();

        var events = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.PolicyId == policy.Id).OrderBy(x => x.Id).ToListAsync();
        Assert.Collection(events,
            item => { Assert.Equal("Warning", item.EventType); Assert.Equal(1, item.PolicyVersion); },
            item => { Assert.Equal("Exceeded", item.EventType); Assert.Equal(2, item.PolicyVersion); },
            item => { Assert.Equal("Resolved", item.EventType); Assert.Equal(3, item.PolicyVersion); });
    }

    [Fact]
    public async Task DeletedQuotaPolicyResolvesLastActiveThresholdEventOnlyOnce()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"QD-{suffix}",
            name = "삭제 정책 해제 테스트 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        await using var scope = factory.Services.CreateAsyncScope();
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var dataSetDb = scope.ServiceProvider.GetRequiredService<DataSetDbContext>();
        var storageAdapter = scope.ServiceProvider.GetRequiredService<IDataSetStorageAdapter>();
        var snapshotService = scope.ServiceProvider.GetRequiredService<DataSetStorageUsageSnapshotService>();

        var allocation = new DataSetStorageAllocation(
            new EDAN.Tensor.DataSet.Domain.Model.DataSetId(Guid.NewGuid()),
            new EDAN.Tensor.DataSet.Domain.Model.DataSetScope(tenantId, Guid.NewGuid(), Guid.NewGuid()));
        var storageReference = await storageAdapter.AllocateAsync(allocation);
        await using (var content = new MemoryStream(new byte[45]))
            await storageAdapter.WriteAsync(allocation, storageReference, content,
                new DataSetStorageQuotaPolicy(null, null, null, null));

        var policy = StorageQuotaPolicy.Create(
            Guid.NewGuid(), new TenantId(tenantId), StorageQuotaPolicyScope.Tenant, tenantId,
            tenantLimitBytes: 50, organizationLimitBytes: null, projectLimitBytes: null, dataSetLimitBytes: null,
            warningThresholdPercent: 80, planCode: "Community", updatedBy: "integration-test",
            nowUtc: DateTimeOffset.UtcNow);
        platformDb.StorageQuotaPolicies.Add(policy);
        await platformDb.SaveChangesAsync();

        await snapshotService.CaptureDueAsync();
        var warning = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .SingleAsync(x => x.PolicyId == policy.Id);
        Assert.Equal("Warning", warning.EventType);

        platformDb.StorageQuotaPolicies.Remove(policy);
        await platformDb.SaveChangesAsync();

        await snapshotService.CaptureDueAsync();
        await snapshotService.CaptureDueAsync();

        var events = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.PolicyId == policy.Id).OrderBy(x => x.Id).ToListAsync();
        Assert.Collection(events,
            item => Assert.Equal("Warning", item.EventType),
            item => Assert.Equal("Resolved", item.EventType));
    }

    [Fact]
    public async Task RemovingQuotaLimitResolvesActiveThresholdEventOnlyOnce()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");

        var suffix = Guid.NewGuid().ToString("N")[..8];
        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"QR-{suffix}",
            name = "Quota 한도 제거 테스트 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        await using var scope = factory.Services.CreateAsyncScope();
        var platformDb = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var dataSetDb = scope.ServiceProvider.GetRequiredService<DataSetDbContext>();
        var storageAdapter = scope.ServiceProvider.GetRequiredService<IDataSetStorageAdapter>();
        var snapshotService = scope.ServiceProvider.GetRequiredService<DataSetStorageUsageSnapshotService>();

        var allocation = new DataSetStorageAllocation(
            new EDAN.Tensor.DataSet.Domain.Model.DataSetId(Guid.NewGuid()),
            new EDAN.Tensor.DataSet.Domain.Model.DataSetScope(tenantId, Guid.NewGuid(), Guid.NewGuid()));
        var storageReference = await storageAdapter.AllocateAsync(allocation);
        await using (var content = new MemoryStream(new byte[45]))
            await storageAdapter.WriteAsync(allocation, storageReference, content,
                new DataSetStorageQuotaPolicy(null, null, null, null));

        var policy = StorageQuotaPolicy.Create(
            Guid.NewGuid(), new TenantId(tenantId), StorageQuotaPolicyScope.Tenant, tenantId,
            tenantLimitBytes: 50, organizationLimitBytes: null, projectLimitBytes: null, dataSetLimitBytes: null,
            warningThresholdPercent: 80, planCode: "Community", updatedBy: "integration-test",
            nowUtc: DateTimeOffset.UtcNow);
        platformDb.StorageQuotaPolicies.Add(policy);
        await platformDb.SaveChangesAsync();

        await snapshotService.CaptureDueAsync();
        Assert.Equal("Warning", (await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .SingleAsync(x => x.PolicyId == policy.Id)).EventType);

        policy.Update(tenantLimitBytes: null, organizationLimitBytes: null, projectLimitBytes: null,
            dataSetLimitBytes: null, warningThresholdPercent: 80, planCode: "Community",
            updatedBy: "integration-test", expectedVersion: policy.Version, nowUtc: DateTimeOffset.UtcNow);
        await platformDb.SaveChangesAsync();

        await snapshotService.CaptureDueAsync();
        await snapshotService.CaptureDueAsync();

        var events = await dataSetDb.QuotaThresholdEvents.AsNoTracking()
            .Where(x => x.PolicyId == policy.Id).OrderBy(x => x.Id).ToListAsync();
        Assert.Collection(events,
            item => { Assert.Equal("Warning", item.EventType); Assert.Equal(1, item.PolicyVersion); },
            item => { Assert.Equal("Resolved", item.EventType); Assert.Equal(2, item.PolicyVersion); });
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

    /// <summary>Tenant 선택 헤더가 없으면 보호 API가 업무 처리 전에 일관된 거부 오류를 반환합니다.</summary>
    [Fact]
    public async Task TenantScopedApiRequiresTenantSelectionHeader()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");

        using var response = await client.PostAsJsonAsync("/api/platform/resources", new
        {
            organizationId = (Guid?)null,
            projectId = (Guid?)null,
            resourceType = "DataSet",
            name = "Tenant 헤더 누락"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("platform.tenant.forbidden", await ReadErrorCodeAsync(response));
    }

    /// <summary>저장 용량 정책 API가 Tenant 관리자·페이지·감사·낙관적 동시성 경계를 함께 적용합니다.</summary>
    [Fact]
    public async Task StorageQuotaPolicyApiSupportsPagingAuditingAndOptimisticConcurrency()
    {
        using var factory = new ApiHostFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Subject", "platform-admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "platform_admin");
        var suffix = Guid.NewGuid().ToString("N")[..8];

        using var tenantResponse = await client.PostAsJsonAsync("/api/platform/tenants", new
        {
            code = $"QUOTA-{suffix}",
            name = "Quota API 테스트 Tenant"
        });
        Assert.Equal(HttpStatusCode.Created, tenantResponse.StatusCode);
        var tenantId = await ReadIdAsync(tenantResponse);

        using var activation = await client.PostAsync($"/api/platform/tenants/{tenantId}/activate", null);
        Assert.Equal(HttpStatusCode.NoContent, activation.StatusCode);
        using var membership = await client.PostAsJsonAsync($"/api/platform/tenants/{tenantId}/memberships", new
        {
            subjectId = "quota-admin",
            role = "Administrator"
        });
        Assert.Equal(HttpStatusCode.Created, membership.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "quota-admin");
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        using var create = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies", new
            {
                scope = "Tenant",
                targetId = tenantId,
                tenantLimitBytes = 1024L,
                organizationLimitBytes = (long?)null,
                projectLimitBytes = (long?)null,
                dataSetLimitBytes = (long?)null,
                warningThresholdPercent = 75,
                planCode = "Community"
            });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createdDocument = await JsonDocument.ParseAsync(await create.Content.ReadAsStreamAsync());
        var policyId = createdDocument.RootElement.GetProperty("id").GetGuid();
        Assert.Equal(1, createdDocument.RootElement.GetProperty("version").GetInt64());

        using var duplicate = await client.PostAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies", new
            {
                scope = "Tenant",
                targetId = tenantId,
                tenantLimitBytes = 2048L,
                warningThresholdPercent = 80,
                planCode = "Community"
            });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("quota_policy.conflict", await ReadErrorCodeAsync(duplicate));

        using var list = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies?page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        using var listDocument = await JsonDocument.ParseAsync(await list.Content.ReadAsStreamAsync());
        Assert.Equal(1, listDocument.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, listDocument.RootElement.GetProperty("items").GetArrayLength());

        using var update = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}", new
            {
                expectedVersion = 1L,
                tenantLimitBytes = 2048L,
                organizationLimitBytes = (long?)null,
                projectLimitBytes = (long?)null,
                dataSetLimitBytes = (long?)null,
                warningThresholdPercent = 80,
                planCode = "Community"
            });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        using var updateDocument = await JsonDocument.ParseAsync(await update.Content.ReadAsStreamAsync());
        Assert.Equal(2, updateDocument.RootElement.GetProperty("version").GetInt64());

        using var history = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}/history?page=1&pageSize=1");
        Assert.True(history.IsSuccessStatusCode, await history.Content.ReadAsStringAsync());
        using var historyDocument = await JsonDocument.ParseAsync(await history.Content.ReadAsStreamAsync());
        Assert.Equal(2, historyDocument.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, historyDocument.RootElement.GetProperty("items").GetArrayLength());

        using var disable = new HttpRequestMessage(HttpMethod.Patch,
            $"/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}/enabled")
        {
            Content = JsonContent.Create(new { isEnabled = false, expectedVersion = 2L })
        };
        using var disabled = await client.SendAsync(disable);
        Assert.Equal(HttpStatusCode.OK, disabled.StatusCode);
        using var disabledDocument = await JsonDocument.ParseAsync(await disabled.Content.ReadAsStreamAsync());
        Assert.False(disabledDocument.RootElement.GetProperty("isEnabled").GetBoolean());
        Assert.Equal(3, disabledDocument.RootElement.GetProperty("version").GetInt64());

        using var staleUpdate = await client.PutAsJsonAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies/{policyId}", new
            {
                expectedVersion = 1L,
                tenantLimitBytes = 4096L,
                warningThresholdPercent = 80,
                planCode = "Community"
            });
        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
        Assert.Equal("quota_policy.version_conflict", await ReadErrorCodeAsync(staleUpdate));

        using var invalidPage = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies?pageSize=0");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Add("X-Test-Subject", "quota-member-without-membership");
        using var forbidden = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/storage-quota-policies");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("code").GetString();
    }

    private static async Task<int> ReadArrayLengthAsync(HttpResponseMessage response)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetArrayLength();
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetProperty("id").GetGuid();
    }

    /// <summary>짧은 통합 검증 작업에서 Lease 갱신을 계속 허용합니다.</summary>
    private sealed class SuccessfulLeaseRenewer : IWorkerTaskLeaseRenewer
    {
        public Task<bool> RenewAsync(Guid tenantId, Guid taskId, Guid leaseId, long fencingToken,
            DateTimeOffset nowUtc, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    /// <summary>각 테스트에 독립된 SQLite 데이터베이스와 테스트 전용 인증 방식을 제공합니다.</summary>
    private sealed class ApiHostFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-api-{Guid.NewGuid():N}.db");
        private readonly string _archivePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-archives-{Guid.NewGuid():N}");
        private readonly string _workerDatabasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-worker-{Guid.NewGuid():N}.db");
        private readonly string _dataSetDatabasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-dataset-{Guid.NewGuid():N}.db");
        private readonly string _dataSetContentDatabasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-dataset-content-{Guid.NewGuid():N}.db");

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PlatformDatabase"] = $"Data Source={_databasePath};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:WorkerDatabase"] = $"Data Source={_workerDatabasePath};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:DataSetDatabase"] = $"Data Source={_dataSetDatabasePath};Cache=Shared;Pooling=False",
                    ["ConnectionStrings:DataSetContentDatabase"] = $"Data Source={_dataSetContentDatabasePath};Cache=Shared;Pooling=False",
                    ["DataSetContent:MaxUploadBytes"] = "1024",
                    ["DataSetContent:Quota:TenantLimitBytes"] = "50",
                    ["DataSetContent:Quota:OrganizationLimitBytes"] = "100",
                    ["DataSetContent:Quota:ProjectLimitBytes"] = "100",
                    ["DataSetContent:Quota:DataSetLimitBytes"] = "100",
                    ["Database:ApplyMigrationsInDevelopment"] = "true",
                    ["AuthorizationAuditArchive:RootPath"] = _archivePath,
                    ["Authentication:Mode"] = "External",
                    ["Authentication:Issuer"] = "https://identity.test.edan.tensor",
                    ["Authentication:Audience"] = "edan-tensor-api",
                    ["Authentication:SigningKey"] = "integration-test-signing-key-at-least-32-bytes-long",
                    ["ExternalDelivery:AllowLoopback"] = "true"
                });
            });
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IDataSetStorageQuotaPolicyProvider>(new IntegrationQuotaPolicyProvider());
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Integration";
                    options.DefaultChallengeScheme = "Integration";
                }).AddScheme<AuthenticationSchemeOptions, IntegrationAuthenticationHandler>("Integration", _ => { });
            });
        }

        private sealed class IntegrationQuotaPolicyProvider : IDataSetStorageQuotaPolicyProvider
        {
            public Task<DataSetStorageQuotaPolicy> GetPolicyAsync(DataSetStorageAllocation allocation, CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(new DataSetStorageQuotaPolicy(50, 100, 100, 100));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(_databasePath))
                File.Delete(_databasePath);
            if (disposing && File.Exists(_workerDatabasePath))
                File.Delete(_workerDatabasePath);
            if (disposing && File.Exists(_dataSetDatabasePath))
                File.Delete(_dataSetDatabasePath);
            if (disposing && File.Exists(_dataSetContentDatabasePath))
                File.Delete(_dataSetContentDatabasePath);
            if (disposing && Directory.Exists(_archivePath))
                Directory.Delete(_archivePath, recursive: true);
        }
    }
}
