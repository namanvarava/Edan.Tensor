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
using EDAN.Tensor.Worker.Domain.Model;
using EDAN.Tensor.Worker.Infrastructure.Persistence;

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
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-policies", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-policies/{policyId}", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/authorization-policies/{policyId}/enabled", document, StringComparison.Ordinal);
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
        Assert.Contains("/api/platform/tenants/{tenantId}/worker/tasks/{taskId}", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/worker/dead-letters", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/worker/dead-letters/{taskId}/replay", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/organizations/{organizationId}/memberships", document, StringComparison.Ordinal);
        Assert.Contains("/api/platform/tenants/{tenantId}/organizations/{organizationId}/projects/{projectId}/memberships", document, StringComparison.Ordinal);
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

        using var replayResponse = await client.PostAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters/{task.Id}/replay", null);
        Assert.Equal(HttpStatusCode.Created, replayResponse.StatusCode);
        using var replayDocument = await JsonDocument.ParseAsync(await replayResponse.Content.ReadAsStreamAsync());
        var replayTaskId = replayDocument.RootElement.GetProperty("replayTaskId").GetGuid();
        Assert.NotEqual(task.Id, replayTaskId);

        using var repeatedList = await client.GetAsync(
            $"/api/platform/tenants/{tenantId}/worker/dead-letters");
        Assert.Equal(HttpStatusCode.OK, repeatedList.StatusCode);
        using var repeatedDocument = await JsonDocument.ParseAsync(await repeatedList.Content.ReadAsStreamAsync());
        Assert.Equal(task.Id, repeatedDocument.RootElement.GetProperty("items")[0].GetProperty("id").GetGuid());
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

    /// <summary>각 테스트에 독립된 SQLite 데이터베이스와 테스트 전용 인증 방식을 제공합니다.</summary>
    private sealed class ApiHostFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-api-{Guid.NewGuid():N}.db");
        private readonly string _archivePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-archives-{Guid.NewGuid():N}");
        private readonly string _workerDatabasePath = Path.Combine(
            Path.GetTempPath(), $"edan-tensor-worker-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:PlatformDatabase"] = $"Data Source={_databasePath};Cache=Shared",
                    ["ConnectionStrings:WorkerDatabase"] = $"Data Source={_workerDatabasePath};Cache=Shared",
                    ["Database:ApplyMigrationsInDevelopment"] = "true",
                    ["AuthorizationAuditArchive:RootPath"] = _archivePath,
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
            if (disposing && File.Exists(_workerDatabasePath))
                File.Delete(_workerDatabasePath);
            if (disposing && Directory.Exists(_archivePath))
                Directory.Delete(_archivePath, recursive: true);
        }
    }
}
