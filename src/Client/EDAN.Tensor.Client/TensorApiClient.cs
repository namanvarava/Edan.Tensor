using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EDAN.Tensor.Client.Contracts;

namespace EDAN.Tensor.Client;

/// <summary>EDAN Tensor API 호출에 필요한 인증과 Tenant 선택 정보를 제공합니다.</summary>
public sealed class TensorApiClientOptions
{
    /// <summary>Bearer 토큰을 반환하는 선택적 공급자입니다.</summary>
    public Func<CancellationToken, ValueTask<string?>>? AccessTokenProvider { get; init; }

    /// <summary>모든 Tenant 범위 요청에 사용할 기본 Tenant 식별자입니다.</summary>
    public Guid? DefaultTenantId { get; init; }
}

/// <summary>EDAN Tensor API의 비성공 응답을 나타냅니다.</summary>
public sealed class TensorApiException : HttpRequestException
{
    private TensorApiException(HttpStatusCode statusCode, string code, string description)
        : base($"EDAN Tensor API 요청이 실패했습니다. Status={(int)statusCode}, Code={code}, Description={description}")
    {
        StatusCodeValue = statusCode;
        Code = code;
        Description = description;
    }

    /// <summary>HTTP 응답 상태 코드입니다.</summary>
    public HttpStatusCode StatusCodeValue { get; }

    /// <summary>API가 반환한 안정적인 오류 코드입니다.</summary>
    public string Code { get; }

    /// <summary>API가 반환한 한국어 오류 설명입니다.</summary>
    public string Description { get; }

    private static async Task<TensorApiException> CreateAsync(
        HttpResponseMessage response, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken)
    {
        ApiErrorContract? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<ApiErrorContract>(jsonOptions, cancellationToken);
        }
        catch (JsonException)
        {
            // 외부 시스템이 오류 계약을 지키지 않아도 상태 코드와 일반 오류 코드로 안전하게 전달합니다.
        }

        return new TensorApiException(
            response.StatusCode,
            string.IsNullOrWhiteSpace(error?.Code) ? "api.http_error" : error.Code,
            string.IsNullOrWhiteSpace(error?.Description) ? "API 요청을 처리하지 못했습니다." : error.Description);
    }

    internal static Task<TensorApiException> FromResponseAsync(
        HttpResponseMessage response, JsonSerializerOptions jsonOptions, CancellationToken cancellationToken) =>
        CreateAsync(response, jsonOptions, cancellationToken);
}

/// <summary>Tenant·DataSet·Worker 핵심 흐름을 HTTP로 호출하는 typed API Client입니다.</summary>
public sealed class TensorApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly TensorApiClientOptions _options;

    /// <summary>HTTP Client와 선택적 인증·Tenant 설정으로 API Client를 생성합니다.</summary>
    /// <param name="httpClient">BaseAddress가 API Host 주소로 설정된 HTTP Client입니다.</param>
    /// <param name="options">Bearer 토큰 공급자와 기본 Tenant 설정입니다.</param>
    /// <exception cref="ArgumentNullException">HTTP Client가 null이면 발생합니다.</exception>
    public TensorApiClient(HttpClient httpClient, TensorApiClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        if (httpClient.BaseAddress is null)
            throw new ArgumentException("HTTP Client의 BaseAddress가 필요합니다.", nameof(httpClient));

        _httpClient = httpClient;
        _options = options ?? new TensorApiClientOptions();
    }

    /// <summary>현재 인증 주체가 속한 Tenant 목록을 조회합니다.</summary>
    public Task<IReadOnlyList<TenantMembershipContract>> GetMyMembershipsAsync(
        CancellationToken cancellationToken = default) =>
        SendJsonAsync<IReadOnlyList<TenantMembershipContract>>(
            CreateRequest(HttpMethod.Get, "/api/platform/tenants/mine"), cancellationToken);

    /// <summary>선택된 Tenant에 새 DataSet을 생성합니다.</summary>
    public Task<CreateDataSetResponseContract> CreateDataSetAsync(
        Guid tenantId, CreateDataSetRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<CreateDataSetResponseContract>(
            CreateRequest(HttpMethod.Post, TenantPath(tenantId, "datasets"), tenantId), request, cancellationToken);
    }

    /// <summary>선택된 Tenant의 DataSet 상세 정보를 조회합니다.</summary>
    public Task<DataSetDetailsContract?> GetDataSetAsync(
        Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        return SendOptionalAsync<DataSetDetailsContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"datasets/{dataSetId:D}"), tenantId), cancellationToken);
    }

    /// <summary>DataSet에 불변 스키마 revision을 등록합니다.</summary>
    public Task<AddDataSetSchemaResponseContract> AddDataSetSchemaAsync(
        Guid tenantId, Guid dataSetId, AddDataSetSchemaRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AddDataSetSchemaResponseContract>(
            CreateRequest(HttpMethod.Post, TenantPath(tenantId, $"datasets/{dataSetId:D}/schemas"), tenantId),
            request, cancellationToken);
    }

    /// <summary>DataSet 콘텐츠를 스트림으로 저장합니다.</summary>
    /// <param name="tenantId">DataSet이 속한 Tenant 식별자입니다.</param>
    /// <param name="dataSetId">콘텐츠를 저장할 DataSet 식별자입니다.</param>
    /// <param name="content">저장할 콘텐츠 스트림입니다. 요청이 완료되면 Client가 스트림을 해제하므로 이후 재사용하지 않아야 합니다.</param>
    /// <param name="contentType">콘텐츠 MIME 형식입니다.</param>
    public async Task WriteDataSetContentAsync(
        Guid tenantId, Guid dataSetId, Stream content, string contentType = "application/octet-stream",
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        using var request = CreateRequest(
            HttpMethod.Post, TenantPath(tenantId, $"datasets/{dataSetId:D}/content"), tenantId);
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        await SendNoContentAsync(request, cancellationToken);
    }

    /// <summary>DataSet 콘텐츠를 읽기 스트림으로 반환합니다.</summary>
    /// <remarks>반환된 스트림을 Dispose하면 HTTP 응답도 함께 해제됩니다.</remarks>
    public Task<Stream> OpenDataSetContentAsync(
        Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        return SendStreamAsync(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"datasets/{dataSetId:D}/content"), tenantId),
            cancellationToken);
    }

    /// <summary>고정된 DataSet·스키마·프로필 revision으로 검증 작업을 등록합니다.</summary>
    public Task<CreateDataSetValidationResponseContract> CreateDataSetValidationAsync(
        Guid tenantId, Guid dataSetId, CreateDataSetValidationRequestContract request, string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        ArgumentNullException.ThrowIfNull(request);
        ValidateHeaderValue(idempotencyKey, nameof(idempotencyKey));
        var message = CreateRequest(
            HttpMethod.Post, TenantPath(tenantId, $"datasets/{dataSetId:D}/validations"), tenantId);
        message.Headers.Add("Idempotency-Key", idempotencyKey.Trim());
        return SendJsonAsync<CreateDataSetValidationResponseContract>(message, request, cancellationToken);
    }

    /// <summary>검증 실행의 상태를 조회합니다.</summary>
    public Task<DataSetValidationExecutionContract?> GetDataSetValidationAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(executionId, nameof(executionId));
        return SendOptionalAsync<DataSetValidationExecutionContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"datasets/validations/{executionId:D}"), tenantId),
            cancellationToken);
    }

    /// <summary>검증 결과 상세를 페이지 단위로 조회합니다.</summary>
    public Task<DataSetValidationDetailsPageContract> GetDataSetValidationDetailsAsync(
        Guid tenantId, Guid executionId, int skip = 0, int take = 100,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(executionId, nameof(executionId));
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        if (take is < 1 or > 1000) throw new ArgumentOutOfRangeException(nameof(take));
        var path = TenantPath(tenantId, $"datasets/validations/{executionId:D}/details?skip={skip}&take={take}");
        return SendJsonAsync<DataSetValidationDetailsPageContract>(CreateRequest(HttpMethod.Get, path, tenantId), cancellationToken);
    }

    /// <summary>Tenant 관리자가 완료된 검증 실행과 보존된 상세 결과를 명시적으로 삭제합니다.</summary>
    /// <param name="tenantId">검증 실행이 속한 Tenant 식별자입니다.</param>
    /// <param name="executionId">삭제할 검증 실행 식별자입니다.</param>
    /// <param name="cancellationToken">요청 취소를 감시하는 토큰입니다.</param>
    /// <exception cref="ArgumentException">Tenant 또는 검증 실행 식별자가 비어 있으면 발생합니다.</exception>
    /// <exception cref="TensorApiException">검증 실행이 없거나 아직 실행 중이면 API 오류 계약과 함께 발생합니다.</exception>
    public Task DeleteDataSetValidationAsync(
        Guid tenantId, Guid executionId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(executionId, nameof(executionId));
        return SendNoContentAsync(
            CreateRequest(HttpMethod.Delete, TenantPath(tenantId, $"datasets/validations/{executionId:D}"), tenantId),
            cancellationToken);
    }

    /// <summary>활성 Worker 작업 유형을 Tenant 작업으로 등록합니다.</summary>
    public Task<EnqueueWorkerTaskResponseContract> EnqueueWorkerTaskAsync(
        Guid tenantId, EnqueueWorkerTaskRequestContract request, string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateHeaderValue(idempotencyKey, nameof(idempotencyKey));
        var message = CreateRequest(HttpMethod.Post, TenantPath(tenantId, "worker/tasks"), tenantId);
        message.Headers.Add("Idempotency-Key", idempotencyKey.Trim());
        return SendJsonAsync<EnqueueWorkerTaskResponseContract>(message, request, cancellationToken);
    }

    /// <summary>Worker 작업의 Lease·Fencing·진행 상태를 조회합니다.</summary>
    public Task<WorkerTaskStatusContract?> GetWorkerTaskAsync(
        Guid tenantId, Guid taskId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(taskId, nameof(taskId));
        return SendOptionalAsync<WorkerTaskStatusContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"worker/tasks/{taskId:D}"), tenantId), cancellationToken);
    }

    /// <summary>Tenant 관리자가 조회할 DLQ 작업을 페이지 단위로 반환합니다.</summary>
    public Task<WorkerDeadLetterPageContract> GetWorkerDeadLettersAsync(
        Guid tenantId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(pageSize));
        return SendJsonAsync<WorkerDeadLetterPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"worker/dead-letters?page={page}&pageSize={pageSize}"), tenantId),
            cancellationToken);
    }

    /// <summary>DLQ 작업을 멱등적인 Replay-Request-Id로 재처리합니다.</summary>
    public Task<ReplayWorkerDeadLetterResponseContract> ReplayWorkerDeadLetterAsync(
        Guid tenantId, Guid taskId, Guid replayRequestId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(taskId, nameof(taskId));
        ValidateId(replayRequestId, nameof(replayRequestId));
        var message = CreateRequest(
            HttpMethod.Post, TenantPath(tenantId, $"worker/dead-letters/{taskId:D}/replay"), tenantId);
        message.Headers.Add("Replay-Request-Id", replayRequestId.ToString("D"));
        return SendJsonAsync<ReplayWorkerDeadLetterResponseContract>(message, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, Guid? tenantId = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (_options.DefaultTenantId is Guid defaultTenantId)
            request.Headers.Add("X-Tenant-Id", defaultTenantId.ToString("D"));
        if (tenantId is Guid scopedTenantId)
        {
            request.Headers.Remove("X-Tenant-Id");
            request.Headers.Add("X-Tenant-Id", scopedTenantId.ToString("D"));
        }

        return request;
    }

    private async Task<T> SendJsonAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            return await SendJsonResponseAsync<T>(request, cancellationToken);
        }
    }

    private async Task<T> SendJsonAsync<T>(
        HttpRequestMessage request, object content, CancellationToken cancellationToken)
    {
        using (request)
        {
            request.Content = JsonContent.Create(content, options: JsonOptions);
            return await SendJsonResponseAsync<T>(request, cancellationToken);
        }
    }

    private async Task<T> SendJsonResponseAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddAuthenticationAsync(request, cancellationToken);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("API 응답 본문이 비어 있습니다.");
    }

    private async Task<T?> SendOptionalAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            await AddAuthenticationAsync(request, cancellationToken);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default!;
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
                ?? throw new InvalidOperationException("API 응답 본문이 비어 있습니다.");
        }
    }

    private async Task SendNoContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            await AddAuthenticationAsync(request, cancellationToken);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }
    }

    private async Task<Stream> SendStreamAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await AddAuthenticationAsync(request, cancellationToken);
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        try
        {
            await EnsureSuccessAsync(response, cancellationToken);
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return new ResponseContentStream(stream, response, request);
        }
        catch
        {
            response.Dispose();
            request.Dispose();
            throw;
        }
    }

    private async Task AddAuthenticationAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _options.AccessTokenProvider is null
            ? null
            : await _options.AccessTokenProvider(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            throw await TensorApiException.FromResponseAsync(response, JsonOptions, cancellationToken);
    }

    private static string TenantPath(Guid tenantId, string suffix) =>
        $"/api/platform/tenants/{tenantId:D}/{suffix}";

    private static void ValidateTenantId(Guid tenantId) => ValidateId(tenantId, nameof(tenantId));

    private static void ValidateId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("식별자는 비어 있을 수 없습니다.", parameterName);
    }

    private static void ValidateHeaderValue(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Trim().Length > 200)
            throw new ArgumentException("헤더 값은 200자를 초과할 수 없습니다.", parameterName);
    }

    private sealed class ResponseContentStream(Stream inner, HttpResponseMessage response, HttpRequestMessage request)
        : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override ValueTask DisposeAsync()
        {
            inner.Dispose();
            response.Dispose();
            request.Dispose();
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
                response.Dispose();
                request.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

