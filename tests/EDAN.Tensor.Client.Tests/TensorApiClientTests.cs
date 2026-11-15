using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EDAN.Tensor.Client;
using EDAN.Tensor.Client.Contracts;
using Xunit;

namespace EDAN.Tensor.Client.Tests;

public sealed class TensorApiClientTests
{
    /// <summary>소속 조회가 Bearer 토큰과 기본 Tenant 헤더를 전달하고 계약을 역직렬화합니다.</summary>
    [Fact]
    public async Task GetMyMembershipsAsync_SendsAuthenticationAndReadsContract()
    {
        var tenantId = Guid.NewGuid();
        var handler = new RecordingHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/platform/tenants/mine", request.RequestUri!.AbsolutePath);
            Assert.Equal("Bearer token-value", request.Headers.Authorization!.ToString());
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            return JsonResponse("[{\"tenantId\":\"" + tenantId + "\",\"role\":\"platform_admin\"}]");
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient, new TensorApiClientOptions
        {
            DefaultTenantId = tenantId,
            AccessTokenProvider = _ => ValueTask.FromResult<string?>("token-value")
        });

        var memberships = await client.GetMyMembershipsAsync();

        var membership = Assert.Single(memberships);
        Assert.Equal(tenantId, membership.TenantId);
        Assert.Equal("platform_admin", membership.Role);
    }

    [Fact]
    public async Task GetOrganizationsAsync_EncodesDirectoryFiltersAndPaging()
    {
        var tenantId = Guid.NewGuid();
        var handler = new RecordingHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal($"/api/platform/tenants/{tenantId:D}/organizations", request.RequestUri!.AbsolutePath);
            Assert.Contains("search=alpha%20team", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Contains("page=2", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Contains("pageSize=10", request.RequestUri.Query, StringComparison.Ordinal);
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            return JsonResponse($"{{\"items\":[{{\"id\":\"{Guid.NewGuid():D}\",\"tenantId\":\"{tenantId:D}\",\"code\":\"ORG\",\"name\":\"Alpha Team\",\"status\":\"Active\"}}],\"totalCount\":1,\"page\":2,\"pageSize\":10}}");
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        var page = await client.GetOrganizationsAsync(tenantId, "alpha team", page: 2, pageSize: 10);

        Assert.Single(page.Items);
        Assert.Equal("Alpha Team", page.Items[0].Name);
        Assert.Equal(2, page.Page);
    }

    [Fact]
    public async Task TenantScopedOperationalContractsDeserializeAndPreserveHeaders()
    {
        var tenantId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var handler = new RecordingHandler((request, _) =>
        {
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            if (request.RequestUri!.AbsolutePath.EndsWith("authorization-audit", StringComparison.Ordinal))
                return JsonResponse("{\"totalCount\":1,\"page\":1,\"pageSize\":50,\"items\":[{\"id\":\"" + Guid.NewGuid() + "\",\"subjectId\":\"user-1\",\"action\":\"read\",\"resourceType\":\"DataSet\",\"scope\":\"Tenant\",\"isAllowed\":true,\"reasonCode\":\"policy.allow\",\"matchedPolicyId\":null,\"occurredAtUtc\":\"2026-01-01T00:00:00Z\"}]}");
            if (request.RequestUri.AbsolutePath.EndsWith("usage", StringComparison.Ordinal))
                return JsonResponse("{\"tenantBytes\":12,\"tenantDataSetCount\":1,\"organizationBytes\":8,\"organizationDataSetCount\":1,\"projectBytes\":4,\"projectDataSetCount\":1,\"dataSetBytes\":4,\"limits\":{\"tenantLimitBytes\":100,\"organizationLimitBytes\":80,\"projectLimitBytes\":60,\"dataSetLimitBytes\":40},\"utilizationPercent\":{\"tenant\":12,\"organization\":10,\"project\":6.67,\"dataSet\":10},\"warningThresholdPercent\":80,\"warnings\":[],\"measuredAtUtc\":\"2026-01-01T00:00:00Z\"}");
            return JsonResponse("{\"id\":\"" + deliveryId + "\",\"tenantId\":\"" + tenantId + "\",\"targetUri\":\"https://example.test/hook\",\"idempotencyKey\":\"delivery-1\",\"status\":\"Queued\",\"attemptCount\":0,\"maxAttempts\":3,\"lastErrorCode\":null,\"createdAtUtc\":\"2026-01-01T00:00:00Z\",\"completedAtUtc\":null}");
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        var audit = await client.GetAuthorizationAuditAsync(tenantId);
        var usage = await client.GetDataSetStorageUsageAsync(tenantId, Guid.NewGuid());
        var delivery = await client.GetExternalDeliveryAsync(tenantId, deliveryId);

        Assert.Equal("user-1", Assert.Single(audit.Items).SubjectId);
        Assert.Equal(12, usage.TenantBytes);
        Assert.Equal(deliveryId, delivery!.Id);
    }

    [Fact]
    public async Task CreateExternalDeliveryAsync_SendsTenantAndIdempotencyContract()
    {
        var tenantId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var workerTaskId = Guid.NewGuid();
        var handler = new RecordingHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"/api/platform/tenants/{tenantId:D}/worker/deliveries", request.RequestUri!.AbsolutePath);
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            Assert.Equal("desktop-delivery-1", request.Headers.GetValues("Idempotency-Key").Single());
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            Assert.Contains("https://example.test/hook", body, StringComparison.Ordinal);
            return JsonResponse($"{{\"delivery\":{{\"id\":\"{deliveryId:D}\",\"tenantId\":\"{tenantId:D}\",\"targetUri\":\"https://example.test/hook\",\"idempotencyKey\":\"desktop-delivery-1\",\"status\":\"Queued\",\"attemptCount\":0,\"maxAttempts\":3,\"lastErrorCode\":null,\"createdAtUtc\":\"2026-01-01T00:00:00Z\",\"completedAtUtc\":null}},\"workerTaskId\":\"{workerTaskId:D}\",\"created\":true}}");
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);
        using var payload = JsonDocument.Parse("{\"event\":\"screen-test\"}");

        var response = await client.CreateExternalDeliveryAsync(tenantId,
            new CreateExternalDeliveryRequestContract("https://example.test/hook", payload.RootElement, 3),
            "desktop-delivery-1");

        Assert.Equal(deliveryId, response.Delivery.Id);
        Assert.Equal(workerTaskId, response.WorkerTaskId);
    }

    [Fact]
    public async Task AuthorizationAuditOperationalContractsUseTenantScopedRoutes()
    {
        var tenantId = Guid.NewGuid();
        var archiveId = Guid.NewGuid();
        var handler = new RecordingHandler((request, _) =>
        {
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            if (request.RequestUri!.AbsolutePath.EndsWith("/integrity", StringComparison.Ordinal))
                return JsonResponse($"{{\"tenantId\":\"{tenantId:D}\",\"fromUtc\":\"2026-01-01T00:00:00Z\",\"toUtc\":\"2026-01-02T00:00:00Z\",\"recordCount\":2,\"sha256\":\"ABC\"}}");
            return JsonResponse($"{{\"archiveId\":\"{archiveId:D}\",\"tenantId\":\"{tenantId:D}\",\"fromUtc\":\"2026-01-01T00:00:00Z\",\"toUtc\":\"2026-01-02T00:00:00Z\",\"recordCount\":2,\"sha256\":\"ABC\",\"sizeBytes\":128}}");
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);
        var from = DateTimeOffset.Parse("2026-01-01T00:00:00Z");
        var to = DateTimeOffset.Parse("2026-01-02T00:00:00Z");

        var integrity = await client.GetAuthorizationAuditIntegrityAsync(tenantId, from, to);
        var archive = await client.CreateAuthorizationAuditArchiveAsync(tenantId,
            new CreateAuthorizationAuditArchiveRequestContract(from, to));

        Assert.Equal(2, integrity.RecordCount);
        Assert.Equal(archiveId, archive.ArchiveId);
    }

    /// <summary>검증 등록이 경로 Tenant와 Idempotency-Key를 함께 전달합니다.</summary>
    [Fact]
    public async Task CreateDataSetValidationAsync_SendsTenantAndIdempotencyHeaders()
    {
        var tenantId = Guid.NewGuid();
        var dataSetId = Guid.NewGuid();
        var executionId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var handler = new RecordingHandler(async (request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal($"/api/platform/tenants/{tenantId:D}/datasets/{dataSetId:D}/validations", request.RequestUri!.AbsolutePath);
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            Assert.Equal("validation-request-1", request.Headers.GetValues("Idempotency-Key").Single());
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            Assert.Contains("expectedDataSetVersion", body, StringComparison.Ordinal);
            return JsonResponse($"{{\"execution\":{{\"id\":\"{executionId:D}\",\"tenantId\":\"{tenantId:D}\",\"dataSetId\":\"{dataSetId:D}\",\"expectedDataSetVersion\":1,\"schemaVersion\":1,\"profileId\":\"{profileId:D}\",\"profileRevision\":1,\"fencingToken\":0,\"status\":\"Queued\",\"detailReference\":\"detail\",\"processedRecords\":0,\"totalRecords\":0,\"errorCount\":0,\"warningCount\":0,\"detailItemCount\":0,\"detailTruncated\":false,\"ruleCountsJson\":\"{{}}\",\"failureCode\":null,\"failureMessage\":null,\"createdAtUtc\":\"2026-01-01T00:00:00Z\",\"completedAtUtc\":null}},\"workerTaskId\":\"{Guid.NewGuid():D}\",\"created\":true}}");
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        var response = await client.CreateDataSetValidationAsync(
            tenantId,
            dataSetId,
            new CreateDataSetValidationRequestContract(1, 1, profileId, 1),
            " validation-request-1 ");

        Assert.Equal(executionId, response.Execution.Id);
        Assert.True(response.Created);
    }

    /// <summary>API 오류 계약을 TensorApiException으로 보존합니다.</summary>
    [Fact]
    public async Task CreateDataSetAsync_ThrowsStructuredApiException()
    {
        var handler = new RecordingHandler((_, _) =>
        {
            var response = JsonResponse(
                "{\"code\":\"dataset.invalid_request\",\"description\":\"DataSet 요청이 올바르지 않습니다.\"}",
                HttpStatusCode.BadRequest);
            response.Headers.Add("X-Request-Id", "client-error-request-id");
            return response;
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        var exception = await Assert.ThrowsAsync<TensorApiException>(() =>
            client.CreateDataSetAsync(
                Guid.NewGuid(),
                new CreateDataSetRequestContract(Guid.NewGuid(), Guid.NewGuid(), "dataset")));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCodeValue);
        Assert.Equal("dataset.invalid_request", exception.Code);
        Assert.Equal("DataSet 요청이 올바르지 않습니다.", exception.Description);
        Assert.Equal("client-error-request-id", exception.RequestId);
    }

    /// <summary>Tenant 범위의 선택 조회는 404를 null로 변환합니다.</summary>
    [Fact]
    public async Task GetWorkerTaskAsync_ReturnsNullWhenTaskIsNotFound()
    {
        var handler = new RecordingHandler((_, _) => JsonResponse(
            "{\"code\":\"worker.task.not_found\",\"description\":\"작업을 찾을 수 없습니다.\"}",
            HttpStatusCode.NotFound));
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        var result = await client.GetWorkerTaskAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    /// <summary>콘텐츠 읽기 스트림을 닫으면 HTTP 응답 자원도 닫힙니다.</summary>
    [Fact]
    public async Task OpenDataSetContentAsync_ReturnsReadableResponseStream()
    {
        var handler = new RecordingHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("content"))
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        await using var content = await client.OpenDataSetContentAsync(Guid.NewGuid(), Guid.NewGuid());
        using var reader = new StreamReader(content, Encoding.UTF8);

        Assert.Equal("content", await reader.ReadToEndAsync());
    }

    /// <summary>검증 결과 삭제가 Tenant 경로와 DELETE 메서드를 전달하고 빈 응답을 처리합니다.</summary>
    [Fact]
    public async Task DeleteDataSetValidationAsync_SendsTenantScopedDelete()
    {
        var tenantId = Guid.NewGuid();
        var executionId = Guid.NewGuid();
        var handler = new RecordingHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal(
                $"/api/platform/tenants/{tenantId:D}/datasets/validations/{executionId:D}",
                request.RequestUri!.AbsolutePath);
            Assert.Equal(tenantId.ToString("D"), request.Headers.GetValues("X-Tenant-Id").Single());
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test") };
        var client = new TensorApiClient(httpClient);

        await client.DeleteDataSetValidationAsync(tenantId, executionId);
    }

    /// <summary>페이지 입력과 식별자 입력의 경계를 클라이언트에서 먼저 차단합니다.</summary>
    [Fact]
    public async Task ClientRejectsInvalidPagingAndIdentifiers()
    {
        using var httpClient = new HttpClient(new RecordingHandler((_, _) => JsonResponse("{}")))
        {
            BaseAddress = new Uri("https://api.example.test")
        };
        var client = new TensorApiClient(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.GetWorkerTaskAsync(Guid.Empty, Guid.NewGuid()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetDataSetValidationDetailsAsync(Guid.NewGuid(), Guid.NewGuid(), take: 1001));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetDataSetValidationDetailsAsync(Guid.NewGuid(), Guid.NewGuid(), take: 501));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetWorkerDeadLettersAsync(Guid.NewGuid(), pageSize: 101));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetWorkerDeadLettersAsync(Guid.NewGuid(), page: int.MaxValue));
    }

    private static HttpResponseMessage JsonResponse(string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        public RecordingHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
            : this((request, _) => Task.FromResult(handler(request, CancellationToken.None))) { }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) => handler(request, cancellationToken);
    }
}
