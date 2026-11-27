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
    private const string RequestIdHeaderName = "X-Request-Id";

    private TensorApiException(HttpStatusCode statusCode, string code, string description, string? requestId)
        : base($"EDAN Tensor API 요청이 실패했습니다. Status={(int)statusCode}, Code={code}, Description={description}")
    {
        StatusCodeValue = statusCode;
        Code = code;
        Description = description;
        RequestId = requestId;
    }

    /// <summary>HTTP 응답 상태 코드입니다.</summary>
    public HttpStatusCode StatusCodeValue { get; }

    /// <summary>API가 반환한 안정적인 오류 코드입니다.</summary>
    public string Code { get; }

    /// <summary>API가 반환한 한국어 오류 설명입니다.</summary>
    public string Description { get; }

    /// <summary>서버 로그와 장애 문의를 연결하는 응답 요청 식별자입니다. 서버가 헤더를 반환하지 않으면 null입니다.</summary>
    public string? RequestId { get; }

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

        string? requestId = null;
        if (response.Headers.TryGetValues(RequestIdHeaderName, out var requestIds))
        {
            foreach (var candidate in requestIds)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    requestId = candidate;
                    break;
                }
            }
        }

        return new TensorApiException(
            response.StatusCode,
            string.IsNullOrWhiteSpace(error?.Code) ? "api.http_error" : error.Code,
            string.IsNullOrWhiteSpace(error?.Description) ? "API 요청을 처리하지 못했습니다." : error.Description,
            requestId);
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

    public Task<SystemMenuPageContractV1> GetSystemMenusV1Async(Guid tenantId, string? search = null, bool? isEnabled = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        var query = $"?tenantId={tenantId:D}&page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search.Trim())}";
        if (isEnabled is bool enabled) query += $"&isEnabled={enabled.ToString().ToLowerInvariant()}";
        return SendJsonAsync<SystemMenuPageContractV1>(CreateRequest(HttpMethod.Get, $"/api/v1/system/menus{query}", tenantId), cancellationToken);
    }

    public Task<MasterCodePageContractV1> GetMasterCodesV1Async(Guid tenantId, string? groupCode = null, string? search = null, bool? isEnabled = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        var query = $"?tenantId={tenantId:D}&page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(groupCode)) query += $"&groupCode={Uri.EscapeDataString(groupCode.Trim())}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search.Trim())}";
        if (isEnabled is bool enabled) query += $"&isEnabled={enabled.ToString().ToLowerInvariant()}";
        return SendJsonAsync<MasterCodePageContractV1>(CreateRequest(HttpMethod.Get, $"/api/v1/master/codes{query}", tenantId), cancellationToken);
    }

    public Task<BusinessWorkItemPageContractV1> GetBusinessWorkItemsV1Async(Guid tenantId, string? workType = null, string? status = null, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        var query = $"?tenantId={tenantId:D}&page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(workType)) query += $"&workType={Uri.EscapeDataString(workType.Trim())}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status.Trim())}";
        return SendJsonAsync<BusinessWorkItemPageContractV1>(CreateRequest(HttpMethod.Get, $"/api/v1/business/work-items{query}", tenantId), cancellationToken);
    }

    public Task<SystemMenuContractV1> CreateSystemMenuV1Async(Guid tenantId, UpsertSystemMenuRequestV1 request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<SystemMenuContractV1>(CreateRequest(HttpMethod.Post, $"/api/v1/system/menus?tenantId={tenantId:D}", tenantId), request, cancellationToken);
    }

    public Task<MasterCodeContractV1> CreateMasterCodeV1Async(Guid tenantId, UpsertMasterCodeRequestV1 request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<MasterCodeContractV1>(CreateRequest(HttpMethod.Post, $"/api/v1/master/codes?tenantId={tenantId:D}", tenantId), request, cancellationToken);
    }

    public Task<BusinessWorkItemContractV1> CreateBusinessWorkItemV1Async(Guid tenantId, CreateBusinessWorkItemRequestV1 request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<BusinessWorkItemContractV1>(CreateRequest(HttpMethod.Post, $"/api/v1/business/work-items?tenantId={tenantId:D}", tenantId), request, cancellationToken);
    }

    /// <summary>현재 인증 주체가 속한 Tenant 목록을 조회합니다.</summary>
    public Task<IReadOnlyList<TenantMembershipContract>> GetMyMembershipsAsync(
        CancellationToken cancellationToken = default) =>
        SendJsonAsync<IReadOnlyList<TenantMembershipContract>>(
            CreateRequest(HttpMethod.Get, "/api/platform/tenants/mine"), cancellationToken);

    /// <summary>플랫폼 관리자가 검색·상태·페이지 조건으로 Tenant directory를 조회합니다.</summary>
    public Task<PlatformPageContract<TenantDirectoryContract>> GetTenantsAsync(
        string? search = null, string? status = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, pageSize);
        return SendJsonAsync<PlatformPageContract<TenantDirectoryContract>>(
            CreateRequest(HttpMethod.Get, $"/api/platform/tenants{Query(search, status, page, pageSize)}"), cancellationToken);
    }

    /// <summary>선택된 Tenant의 조직 directory를 조회합니다.</summary>
    public Task<PlatformPageContract<OrganizationDirectoryContract>> GetOrganizationsAsync(
        Guid tenantId, string? search = null, string? status = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        return SendJsonAsync<PlatformPageContract<OrganizationDirectoryContract>>(
            CreateRequest(HttpMethod.Get, $"{TenantPath(tenantId, "organizations")}{Query(search, status, page, pageSize)}", tenantId), cancellationToken);
    }

    /// <summary>선택된 Tenant의 프로젝트 directory를 조회합니다.</summary>
    public Task<PlatformPageContract<ProjectDirectoryContract>> GetProjectsAsync(
        Guid tenantId, Guid? organizationId = null, string? search = null, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        var query = Query(search, status, page, pageSize, organizationId is Guid org ? ("organizationId", org.ToString("D")) : null);
        return SendJsonAsync<PlatformPageContract<ProjectDirectoryContract>>(
            CreateRequest(HttpMethod.Get, $"{TenantPath(tenantId, "projects")}{query}", tenantId), cancellationToken);
    }

    /// <summary>선택된 Tenant의 사용자 directory를 조회합니다.</summary>
    public Task<PlatformPageContract<UserDirectoryContract>> GetUsersAsync(
        Guid tenantId, string? search = null, string? status = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        return SendJsonAsync<PlatformPageContract<UserDirectoryContract>>(
            CreateRequest(HttpMethod.Get, $"{TenantPath(tenantId, "users")}{Query(search, status, page, pageSize)}", tenantId), cancellationToken);
    }

    public Task<InvitationPageContract> GetInvitationsAsync(Guid tenantId, string? search = null, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidatePage(page, pageSize);
        return SendJsonAsync<InvitationPageContract>(CreateRequest(HttpMethod.Get,
            $"{TenantPath(tenantId, "invitations")}{Query(search, status, page, pageSize)}", tenantId), cancellationToken);
    }

    /// <summary>Tenant 범위의 멤버십 directory를 조회합니다.</summary>
    public Task<IReadOnlyList<TenantMembershipDirectoryContract>> GetTenantMembershipsAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        return SendJsonAsync<IReadOnlyList<TenantMembershipDirectoryContract>>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, "memberships"), tenantId), cancellationToken);
    }

    public Task<TenantMembershipPageContract> GetTenantMembershipsFilteredAsync(
        Guid tenantId, string? subjectId = null, string? status = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(subjectId)) parts.Add($"subjectId={Uri.EscapeDataString(subjectId.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        return SendJsonAsync<TenantMembershipPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"memberships?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<OrganizationMembershipPageContract> GetOrganizationMembershipsAsync(
        Guid tenantId, Guid organizationId, string? subjectId = null, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidateId(organizationId, nameof(organizationId)); ValidatePage(page, pageSize);
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(subjectId)) query += $"&subjectId={Uri.EscapeDataString(subjectId.Trim())}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status.Trim())}";
        return SendJsonAsync<OrganizationMembershipPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"organizations/{organizationId:D}/memberships?{query}"), tenantId), cancellationToken);
    }

    public Task<ProjectMembershipPageContract> GetProjectMembershipsAsync(
        Guid tenantId, Guid organizationId, Guid projectId, string? subjectId = null, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidateId(organizationId, nameof(organizationId)); ValidateId(projectId, nameof(projectId)); ValidatePage(page, pageSize);
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(subjectId)) query += $"&subjectId={Uri.EscapeDataString(subjectId.Trim())}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status.Trim())}";
        return SendJsonAsync<ProjectMembershipPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"organizations/{organizationId:D}/projects/{projectId:D}/memberships?{query}"), tenantId), cancellationToken);
    }

    /// <summary>Tenant 범위의 권한 정책을 조회합니다.</summary>
    public Task<AuthorizationPolicyPageContract> GetAuthorizationPoliciesAsync(
        Guid tenantId, Guid? organizationId = null, Guid? projectId = null,
        string? subjectId = null, string? action = null, string? effect = null,
        bool? isEnabled = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (organizationId is Guid organization) parts.Add($"organizationId={organization:D}");
        if (projectId is Guid project) parts.Add($"projectId={project:D}");
        if (!string.IsNullOrWhiteSpace(subjectId)) parts.Add($"subjectId={Uri.EscapeDataString(subjectId.Trim())}");
        if (!string.IsNullOrWhiteSpace(action)) parts.Add($"action={Uri.EscapeDataString(action.Trim())}");
        if (!string.IsNullOrWhiteSpace(effect)) parts.Add($"effect={Uri.EscapeDataString(effect.Trim())}");
        if (isEnabled is bool enabled) parts.Add($"isEnabled={enabled.ToString().ToLowerInvariant()}");
        return SendJsonAsync<AuthorizationPolicyPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"authorization-policies?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<AuthorizationDecisionContract> EvaluateAuthorizationPolicyAsync(Guid tenantId,
        EvaluateAuthorizationPolicyRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AuthorizationDecisionContract>(SendRequest(HttpMethod.Post,
            TenantPath(tenantId, "authorization-policies/evaluate"), tenantId, request), cancellationToken);
    }

    /// <summary>Tenant 범위의 권한 감사 기록을 필터와 함께 조회합니다.</summary>
    public Task<AuthorizationAuditPageContract> GetAuthorizationAuditAsync(
        Guid tenantId, string? subjectId = null, string? action = null, bool? isAllowed = null,
        DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(subjectId)) parts.Add($"subjectId={Uri.EscapeDataString(subjectId.Trim())}");
        if (!string.IsNullOrWhiteSpace(action)) parts.Add($"action={Uri.EscapeDataString(action.Trim())}");
        if (isAllowed is bool allowed) parts.Add($"isAllowed={allowed.ToString().ToLowerInvariant()}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={Uri.EscapeDataString(from.ToUniversalTime().ToString("O"))}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={Uri.EscapeDataString(to.ToUniversalTime().ToString("O"))}");
        return SendJsonAsync<AuthorizationAuditPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"authorization-audit?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<AuthorizationAuditIntegrityResultContract> GetAuthorizationAuditIntegrityAsync(
        Guid tenantId, DateTimeOffset fromUtc, DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateDateWindow(fromUtc, toUtc);
        var path = TenantPath(tenantId,
            $"authorization-audit/integrity?fromUtc={UtcQuery(fromUtc)}&toUtc={UtcQuery(toUtc)}");
        return SendJsonAsync<AuthorizationAuditIntegrityResultContract>(
            CreateRequest(HttpMethod.Get, path, tenantId), cancellationToken);
    }

    public Task<AuthorizationAuditArchiveResultContract> CreateAuthorizationAuditArchiveAsync(
        Guid tenantId, CreateAuthorizationAuditArchiveRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateDateWindow(request.FromUtc, request.ToUtc);
        return SendJsonAsync<AuthorizationAuditArchiveResultContract>(
            CreateRequest(HttpMethod.Post, TenantPath(tenantId, "authorization-audit/archives"), tenantId),
            request, cancellationToken);
    }

    public Task<AuthorizationAuditArchiveVerificationContract> VerifyAuthorizationAuditArchiveAsync(
        Guid tenantId, Guid archiveId, VerifyAuthorizationAuditArchiveRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(archiveId, nameof(archiveId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AuthorizationAuditArchiveVerificationContract>(
            CreateRequest(HttpMethod.Post,
                TenantPath(tenantId, $"authorization-audit/archives/{archiveId:D}/verify"), tenantId),
            request, cancellationToken);
    }

    /// <summary>Tenant 관리자 검증을 통과한 감사 아카이브 원문 스트림을 엽니다.</summary>
    public Task<Stream> OpenAuthorizationAuditArchiveAsync(Guid tenantId, Guid archiveId,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(archiveId, nameof(archiveId));
        return SendStreamAsync(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"authorization-audit/archives/{archiveId:D}"), tenantId), cancellationToken);
    }

    public Task<AuthorizationAuditArchiveScheduleContract> GetAuthorizationAuditArchiveScheduleAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        return SendJsonAsync<AuthorizationAuditArchiveScheduleContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, "authorization-audit/archive-schedule"), tenantId),
            cancellationToken);
    }

    public Task<AuthorizationAuditArchiveScheduleContract> UpdateAuthorizationAuditArchiveScheduleAsync(
        Guid tenantId, UpdateAuthorizationAuditArchiveScheduleRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AuthorizationAuditArchiveScheduleContract>(
            SendRequest(HttpMethod.Put, TenantPath(tenantId, "authorization-audit/archive-schedule"), tenantId, request),
            cancellationToken);
    }

    public Task<AuthorizationAuditArchiveJobContract> CreateAuthorizationAuditArchiveJobAsync(
        Guid tenantId, CreateAuthorizationAuditArchiveRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateDateWindow(request.FromUtc, request.ToUtc);
        return SendJsonAsync<AuthorizationAuditArchiveJobContract>(
            CreateRequest(HttpMethod.Post, TenantPath(tenantId, "authorization-audit/archive-jobs"), tenantId),
            request, cancellationToken);
    }

    public Task<AuthorizationAuditArchiveJobDetailsContract?> GetAuthorizationAuditArchiveJobAsync(
        Guid tenantId, Guid jobId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(jobId, nameof(jobId));
        return SendOptionalAsync<AuthorizationAuditArchiveJobDetailsContract>(
            CreateRequest(HttpMethod.Get,
                TenantPath(tenantId, $"authorization-audit/archive-jobs/{jobId:D}"), tenantId),
            cancellationToken);
    }

    public Task<AuthorizationAuditArchiveJobPageContract> GetAuthorizationAuditArchiveJobsAsync(
        Guid tenantId, string? status = null, DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={UtcQuery(from)}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={UtcQuery(to)}");
        return SendJsonAsync<AuthorizationAuditArchiveJobPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId,
                $"authorization-audit/archive-jobs?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<AuthorizationAuditArchiveJobContract> RetryAuthorizationAuditArchiveJobAsync(
        Guid tenantId, Guid jobId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(jobId, nameof(jobId));
        return SendJsonAsync<AuthorizationAuditArchiveJobContract>(
            CreateRequest(HttpMethod.Post,
                TenantPath(tenantId, $"authorization-audit/archive-jobs/{jobId:D}/retry"), tenantId),
            cancellationToken);
    }

    public Task<AuthorizationAuditJobVerificationContract> VerifyAuthorizationAuditArchiveJobAsync(
        Guid tenantId, Guid jobId, VerifyAuthorizationAuditArchiveRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(jobId, nameof(jobId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AuthorizationAuditJobVerificationContract>(
            CreateRequest(HttpMethod.Post,
                TenantPath(tenantId, $"authorization-audit/archive-jobs/{jobId:D}/verify"), tenantId),
            request, cancellationToken);
    }

    public Task<AuthorizationAuditRetentionPolicyContract> GetAuthorizationAuditRetentionAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        return SendJsonAsync<AuthorizationAuditRetentionPolicyContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, "authorization-audit/retention"), tenantId),
            cancellationToken);
    }

    public Task<AuthorizationAuditRetentionPolicyContract> UpdateAuthorizationAuditRetentionAsync(
        Guid tenantId, UpdateAuthorizationAuditRetentionRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AuthorizationAuditRetentionPolicyContract>(
            SendRequest(HttpMethod.Put, TenantPath(tenantId, "authorization-audit/retention"), tenantId, request),
            cancellationToken);
    }

    /// <summary>Tenant 범위의 저장 용량 정책을 페이지로 조회합니다.</summary>
    public Task<StorageQuotaPolicyPageContract> GetStorageQuotaPoliciesAsync(
        Guid tenantId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        return SendJsonAsync<StorageQuotaPolicyPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"storage-quota-policies?page={page}&pageSize={pageSize}"), tenantId), cancellationToken);
    }

    public Task<StorageQuotaPolicyPageContract> GetStorageQuotaPoliciesFilteredAsync(
        Guid tenantId, string? scope = null, bool? isEnabled = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(scope)) query += $"&scope={Uri.EscapeDataString(scope.Trim())}";
        if (isEnabled is bool enabled) query += $"&isEnabled={enabled.ToString().ToLowerInvariant()}";
        return SendJsonAsync<StorageQuotaPolicyPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"storage-quota-policies?{query}"), tenantId),
            cancellationToken);
    }

    public Task<StorageQuotaPolicyPageContract> GetStorageQuotaPoliciesByTargetAsync(
        Guid tenantId, string? scope = null, Guid? targetId = null, bool? isEnabled = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(scope)) query += $"&scope={Uri.EscapeDataString(scope.Trim())}";
        if (targetId is Guid target) query += $"&targetId={target:D}";
        if (isEnabled is bool enabled) query += $"&isEnabled={enabled.ToString().ToLowerInvariant()}";
        return SendJsonAsync<StorageQuotaPolicyPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"storage-quota-policies?{query}"), tenantId), cancellationToken);
    }

    public Task SetUserStatusAsync(Guid tenantId, Guid userId, SetUserStatusRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(userId, nameof(userId));
        ArgumentNullException.ThrowIfNull(request);
        return SendNoContentAsync(SendRequest(HttpMethod.Patch,
            TenantPath(tenantId, $"users/{userId:D}/status"), tenantId, request), cancellationToken);
    }

    /// <summary>DataSet 단위 실제 저장량과 적용된 한도를 조회합니다.</summary>
    public Task<DataSetStorageUsageContract> GetDataSetStorageUsageAsync(
        Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        return SendJsonAsync<DataSetStorageUsageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"datasets/{dataSetId:D}/usage"), tenantId), cancellationToken);
    }

    /// <summary>Tenant 전체의 실제 저장량을 조회합니다.</summary>
    public Task<DataSetTenantStorageUsageContract> GetTenantStorageUsageAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        return SendJsonAsync<DataSetTenantStorageUsageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, "storage-usage"), tenantId), cancellationToken);
    }

    /// <summary>Tenant 저장량 스냅샷 이력을 조회합니다.</summary>
    public Task<IReadOnlyList<DataSetStorageUsageSnapshotContract>> GetTenantStorageUsageHistoryAsync(
        Guid tenantId, int take = 168, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        if (take is < 1 or > 744) throw new ArgumentOutOfRangeException(nameof(take));
        return SendJsonAsync<IReadOnlyList<DataSetStorageUsageSnapshotContract>>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"storage-usage/history?take={take}"), tenantId), cancellationToken);
    }

    public Task<IReadOnlyList<DataSetStorageUsageSnapshotContract>> GetTenantStorageUsageHistoryFilteredAsync(
        Guid tenantId, int take = 168, DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        if (take is < 1 or > 744) throw new ArgumentOutOfRangeException(nameof(take));
        var query = $"take={take}";
        if (fromUtc is DateTimeOffset from) query += $"&fromUtc={UtcQuery(from)}";
        if (toUtc is DateTimeOffset to) query += $"&toUtc={UtcQuery(to)}";
        return SendJsonAsync<IReadOnlyList<DataSetStorageUsageSnapshotContract>>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"storage-usage/history?{query}"), tenantId), cancellationToken);
    }

    /// <summary>Tenant 저장량 quota 임계치 이벤트 이력을 조회합니다.</summary>
    public Task<IReadOnlyList<DataSetQuotaEventContract>> GetTenantQuotaEventHistoryAsync(
        Guid tenantId, int take = 100, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        if (take is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(take));
        return SendJsonAsync<IReadOnlyList<DataSetQuotaEventContract>>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"storage-usage/quota-events?take={take}"), tenantId), cancellationToken);
    }

    public Task<IReadOnlyList<DataSetQuotaEventContract>> GetTenantQuotaEventHistoryFilteredAsync(
        Guid tenantId, int take = 100, string? scope = null, string? eventType = null,
        DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        if (take is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(take));
        var query = $"take={take}";
        if (!string.IsNullOrWhiteSpace(scope)) query += $"&scope={Uri.EscapeDataString(scope.Trim())}";
        if (!string.IsNullOrWhiteSpace(eventType)) query += $"&eventType={Uri.EscapeDataString(eventType.Trim())}";
        if (fromUtc is DateTimeOffset from) query += $"&fromUtc={UtcQuery(from)}";
        if (toUtc is DateTimeOffset to) query += $"&toUtc={UtcQuery(to)}";
        return SendJsonAsync<IReadOnlyList<DataSetQuotaEventContract>>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"storage-usage/quota-events?{query}"), tenantId),
            cancellationToken);
    }

    /// <summary>Tenant 범위 외부 전달 상태를 조회합니다.</summary>
    public Task<ExternalDeliveryContract?> GetExternalDeliveryAsync(
        Guid tenantId, Guid deliveryId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(deliveryId, nameof(deliveryId));
        return SendOptionalAsync<ExternalDeliveryContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"worker/deliveries/{deliveryId:D}"), tenantId), cancellationToken);
    }

    public Task<ExternalDeliveryPageContract> GetExternalDeliveriesAsync(
        Guid tenantId, string? status = null, string? targetHost = null, string? errorCode = null,
        DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (!string.IsNullOrWhiteSpace(targetHost)) parts.Add($"targetHost={Uri.EscapeDataString(targetHost.Trim())}");
        if (!string.IsNullOrWhiteSpace(errorCode)) parts.Add($"errorCode={Uri.EscapeDataString(errorCode.Trim())}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={UtcQuery(from)}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={UtcQuery(to)}");
        return SendJsonAsync<ExternalDeliveryPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"worker/deliveries?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<ResourceIdContract> CreateTenantAsync(
        CreateTenantRequestContract request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post, "/api/platform/tenants"), request, cancellationToken);
    }

    public Task ActivateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        return SendNoContentAsync(CreateRequest(HttpMethod.Post, $"/api/platform/tenants/{tenantId:D}/activate"), cancellationToken);
    }

    public Task<ResourceIdContract> CreateOrganizationAsync(
        Guid tenantId, CreateOrganizationRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post, TenantPath(tenantId, "organizations"), tenantId), request, cancellationToken);
    }

    public Task<ResourceIdContract> CreateProjectAsync(
        Guid tenantId, Guid organizationId, CreateProjectRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(organizationId, nameof(organizationId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"organizations/{organizationId:D}/projects"), tenantId), request, cancellationToken);
    }

    public Task<ResourceIdContract> InviteUserAsync(
        Guid tenantId, InviteUserRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, "users/invitations"), tenantId), request, cancellationToken);
    }

    public Task<InvitationContract> ResendInvitationAsync(Guid tenantId, Guid invitationId,
        InvitationLifetimeRequestContract? request = null, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidateId(invitationId, nameof(invitationId));
        return SendJsonAsync<InvitationContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"invitations/{invitationId:D}/resend"), tenantId),
            request ?? new InvitationLifetimeRequestContract(), cancellationToken);
    }

    public Task RevokeInvitationAsync(Guid tenantId, Guid invitationId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidateId(invitationId, nameof(invitationId));
        return SendNoContentAsync(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"invitations/{invitationId:D}/revoke"), tenantId), cancellationToken);
    }

    public Task<ResourceIdContract> GrantTenantMembershipAsync(
        Guid tenantId, GrantTenantMembershipRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, "memberships"), tenantId), request, cancellationToken);
    }

    public Task<ResourceIdContract> GrantOrganizationMembershipAsync(
        Guid tenantId, Guid organizationId, GrantOrganizationMembershipRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(organizationId, nameof(organizationId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"organizations/{organizationId:D}/memberships"), tenantId), request, cancellationToken);
    }

    public Task<ResourceIdContract> GrantProjectMembershipAsync(
        Guid tenantId, Guid organizationId, Guid projectId, GrantProjectMembershipRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(organizationId, nameof(organizationId));
        ValidateId(projectId, nameof(projectId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"organizations/{organizationId:D}/projects/{projectId:D}/memberships"), tenantId), request, cancellationToken);
    }

    public Task UpdateTenantMembershipAsync(Guid tenantId, Guid membershipId, UpdateMembershipRequestContract request,
        CancellationToken cancellationToken = default) =>
        UpdateMembershipAsync(tenantId, $"memberships/{ValidateAndFormatId(membershipId, nameof(membershipId))}", request, cancellationToken);

    public Task UpdateOrganizationMembershipAsync(Guid tenantId, Guid organizationId, Guid membershipId,
        UpdateMembershipRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateId(organizationId, nameof(organizationId));
        return UpdateMembershipAsync(tenantId, $"organizations/{organizationId:D}/memberships/{ValidateAndFormatId(membershipId, nameof(membershipId))}", request, cancellationToken);
    }

    public Task UpdateProjectMembershipAsync(Guid tenantId, Guid organizationId, Guid projectId, Guid membershipId,
        UpdateMembershipRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateId(organizationId, nameof(organizationId));
        ValidateId(projectId, nameof(projectId));
        return UpdateMembershipAsync(tenantId, $"organizations/{organizationId:D}/projects/{projectId:D}/memberships/{ValidateAndFormatId(membershipId, nameof(membershipId))}", request, cancellationToken);
    }

    public Task UpdateTenantAsync(Guid tenantId, UpdateDirectoryEntryRequestContract request,
        CancellationToken cancellationToken = default) =>
        UpdateDirectoryEntryAsync(tenantId, string.Empty, request, cancellationToken);

    public Task UpdateOrganizationAsync(Guid tenantId, Guid organizationId, UpdateDirectoryEntryRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(organizationId, nameof(organizationId));
        return UpdateDirectoryEntryAsync(tenantId, $"organizations/{organizationId:D}", request, cancellationToken);
    }

    public Task UpdateProjectAsync(Guid tenantId, Guid organizationId, Guid projectId, UpdateDirectoryEntryRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(organizationId, nameof(organizationId));
        ValidateId(projectId, nameof(projectId));
        return UpdateDirectoryEntryAsync(tenantId, $"organizations/{organizationId:D}/projects/{projectId:D}", request, cancellationToken);
    }

    public Task UpdateUserAsync(Guid tenantId, Guid userId, UpdateDirectoryEntryRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(userId, nameof(userId));
        return UpdateDirectoryEntryAsync(tenantId, $"users/{userId:D}", request, cancellationToken);
    }

    public Task<ResourceIdContract> CreateAuthorizationPolicyAsync(Guid tenantId,
        CreateAuthorizationPolicyRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<ResourceIdContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, "authorization-policies"), tenantId), request, cancellationToken);
    }

    public Task UpdateAuthorizationPolicyAsync(Guid tenantId, Guid policyId,
        UpdateAuthorizationPolicyRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(policyId, nameof(policyId));
        ArgumentNullException.ThrowIfNull(request);
        return SendNoContentAsync(SendRequest(HttpMethod.Put,
            TenantPath(tenantId, $"authorization-policies/{policyId:D}"), tenantId, request), cancellationToken);
    }

    public Task SetAuthorizationPolicyEnabledAsync(Guid tenantId, Guid policyId,
        SetAuthorizationPolicyEnabledRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(policyId, nameof(policyId));
        ArgumentNullException.ThrowIfNull(request);
        return SendNoContentAsync(SendRequest(HttpMethod.Patch,
            TenantPath(tenantId, $"authorization-policies/{policyId:D}/enabled"), tenantId, request), cancellationToken);
    }

    public Task<StorageQuotaPolicyContract> CreateStorageQuotaPolicyAsync(Guid tenantId,
        CreateStorageQuotaPolicyRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<StorageQuotaPolicyContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, "storage-quota-policies"), tenantId), request, cancellationToken);
    }

    public Task<StorageQuotaPolicyContract> UpdateStorageQuotaPolicyAsync(Guid tenantId, Guid policyId,
        UpdateStorageQuotaPolicyRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(policyId, nameof(policyId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<StorageQuotaPolicyContract>(SendRequest(HttpMethod.Put,
            TenantPath(tenantId, $"storage-quota-policies/{policyId:D}"), tenantId, request), cancellationToken);
    }

    public Task<StorageQuotaPolicyContract> SetStorageQuotaPolicyEnabledAsync(Guid tenantId, Guid policyId,
        SetStorageQuotaPolicyEnabledRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(policyId, nameof(policyId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<StorageQuotaPolicyContract>(SendRequest(HttpMethod.Patch,
            TenantPath(tenantId, $"storage-quota-policies/{policyId:D}/enabled"), tenantId, request), cancellationToken);
    }

    public Task<StorageQuotaPolicyHistoryPageContract> GetStorageQuotaPolicyHistoryAsync(Guid tenantId, Guid policyId,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(policyId, nameof(policyId));
        ValidatePage(page, pageSize);
        return SendJsonAsync<StorageQuotaPolicyHistoryPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"storage-quota-policies/{policyId:D}/history?page={page}&pageSize={pageSize}"), tenantId), cancellationToken);
    }

    public Task<CreateDataSetValidationProfileContract> CreateDataSetValidationProfileAsync(Guid tenantId,
        CreateDataSetValidationProfileRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<CreateDataSetValidationProfileContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, "datasets/validation-profiles"), tenantId), request, cancellationToken);
    }

    public Task<CreateDataSetValidationProfileRevisionContract> AddDataSetValidationProfileRevisionAsync(Guid tenantId,
        Guid profileId, CreateDataSetValidationRevisionRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(profileId, nameof(profileId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<CreateDataSetValidationProfileRevisionContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"datasets/validation-profiles/{profileId:D}/revisions"), tenantId), request, cancellationToken);
    }

    public Task<CreateDataSetValidationProfileContract> UpdateDataSetValidationProfileAsync(Guid tenantId,
        Guid profileId, UpdateDataSetValidationProfileRequestContract request,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(profileId, nameof(profileId));
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<CreateDataSetValidationProfileContract>(SendRequest(HttpMethod.Put,
            TenantPath(tenantId, $"datasets/validation-profiles/{profileId:D}"), tenantId, request), cancellationToken);
    }

    public Task DeleteDataSetValidationProfileAsync(Guid tenantId, Guid profileId,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(profileId, nameof(profileId));
        return SendNoContentAsync(CreateRequest(HttpMethod.Delete,
            TenantPath(tenantId, $"datasets/validation-profiles/{profileId:D}"), tenantId), cancellationToken);
    }

    public Task ActivateDataSetAsync(Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        return SendNoContentAsync(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"datasets/{dataSetId:D}/activate"), tenantId), cancellationToken);
    }

    public Task<AuthorizationAuditIntegrityVerificationContract> VerifyAuthorizationAuditIntegrityAsync(Guid tenantId,
        VerifyAuthorizationAuditIntegrityRequestContract request, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendJsonAsync<AuthorizationAuditIntegrityVerificationContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, "authorization-audit/integrity/verify"), tenantId), request, cancellationToken);
    }

    public Task<CreateExternalDeliveryResponseContract> CreateExternalDeliveryAsync(Guid tenantId,
        CreateExternalDeliveryRequestContract request, string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateHeaderValue(idempotencyKey, nameof(idempotencyKey));
        var message = CreateRequest(HttpMethod.Post, TenantPath(tenantId, "worker/deliveries"), tenantId);
        message.Headers.Add("Idempotency-Key", idempotencyKey.Trim());
        return SendJsonAsync<CreateExternalDeliveryResponseContract>(message, request, cancellationToken);
    }

    public Task<CreateExternalDeliveryResponseContract> RetryExternalDeliveryAsync(
        Guid tenantId, Guid deliveryId, string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(deliveryId, nameof(deliveryId));
        ValidateHeaderValue(idempotencyKey, nameof(idempotencyKey));
        var message = CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"worker/deliveries/{deliveryId:D}/retry"), tenantId);
        message.Headers.Add("Idempotency-Key", idempotencyKey.Trim());
        return SendJsonAsync<CreateExternalDeliveryResponseContract>(message, cancellationToken);
    }

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

    /// <summary>Tenant 범위 DataSet을 조직·프로젝트·이름·상태로 필터링해 페이지 조회합니다.</summary>
    public Task<DataSetPageContract> GetDataSetsAsync(Guid tenantId, Guid? organizationId = null,
        Guid? projectId = null, string? search = null, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (organizationId is Guid organization) parts.Add($"organizationId={organization:D}");
        if (projectId is Guid project) parts.Add($"projectId={project:D}");
        if (!string.IsNullOrWhiteSpace(search)) parts.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        return SendJsonAsync<DataSetPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"datasets?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    /// <summary>DataSet의 불변 스키마 revision 목록을 조회합니다.</summary>
    public Task<DataSetSchemaRevisionPageContract> GetDataSetSchemaRevisionsAsync(
        Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        return SendJsonAsync<DataSetSchemaRevisionPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"datasets/{dataSetId:D}/schemas"), tenantId), cancellationToken);
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

    public Task RenameDataSetAsync(Guid tenantId, Guid dataSetId, string name,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return SendNoContentAsync(SendRequest(HttpMethod.Patch,
            TenantPath(tenantId, $"datasets/{dataSetId:D}"), tenantId, new { Name = name.Trim() }), cancellationToken);
    }

    public Task ArchiveDataSetAsync(Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(CreateRequest(HttpMethod.Post, TenantPath(tenantId, $"datasets/{dataSetId:D}/archive"), tenantId), cancellationToken);

    public Task RestoreDataSetAsync(Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(CreateRequest(HttpMethod.Post, TenantPath(tenantId, $"datasets/{dataSetId:D}/restore"), tenantId), cancellationToken);

    public Task ResetDataSetStorageAsync(Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(CreateRequest(HttpMethod.Post, TenantPath(tenantId, $"datasets/{dataSetId:D}/storage/reset"), tenantId), cancellationToken);

    public Task DeleteDataSetAsync(Guid tenantId, Guid dataSetId, CancellationToken cancellationToken = default) =>
        SendNoContentAsync(CreateRequest(HttpMethod.Delete, TenantPath(tenantId, $"datasets/{dataSetId:D}"), tenantId), cancellationToken);

    public Task<DataSetSchemaDiffContract> GetDataSetSchemaDiffAsync(Guid tenantId, Guid dataSetId,
        int fromVersion, int toVersion, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        if (fromVersion < 1) throw new ArgumentOutOfRangeException(nameof(fromVersion));
        if (toVersion < 1) throw new ArgumentOutOfRangeException(nameof(toVersion));
        return SendJsonAsync<DataSetSchemaDiffContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"datasets/{dataSetId:D}/schemas/diff?fromVersion={fromVersion}&toVersion={toVersion}"), tenantId), cancellationToken);
    }

    public Task<AddDataSetSchemaResponseContract> RollbackDataSetSchemaAsync(Guid tenantId, Guid dataSetId,
        int version, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(dataSetId, nameof(dataSetId));
        if (version < 1) throw new ArgumentOutOfRangeException(nameof(version));
        return SendJsonAsync<AddDataSetSchemaResponseContract>(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"datasets/{dataSetId:D}/schemas/{version}/rollback"), tenantId), cancellationToken);
    }

    public Task<DataSetValidationProfilePageContract> GetDataSetValidationProfilesAsync(
        Guid tenantId, string? search = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var query = $"page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search.Trim())}";
        return SendJsonAsync<DataSetValidationProfilePageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"datasets/validation-profiles?{query}"), tenantId), cancellationToken);
    }

    public Task<DataSetValidationExecutionPageContract> GetDataSetValidationsAsync(
        Guid tenantId, Guid? dataSetId = null, Guid? profileId = null, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (dataSetId is Guid dataSet) parts.Add($"dataSetId={dataSet:D}");
        if (profileId is Guid profile) parts.Add($"profileId={profile:D}");
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        return SendJsonAsync<DataSetValidationExecutionPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"datasets/validations?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<DataSetValidationExecutionPageContract> GetDataSetValidationsFilteredAsync(
        Guid tenantId, Guid? dataSetId = null, Guid? profileId = null, string? status = null,
        DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null, string? failureCode = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (dataSetId is Guid dataSet) parts.Add($"dataSetId={dataSet:D}");
        if (profileId is Guid profile) parts.Add($"profileId={profile:D}");
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={UtcQuery(from)}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={UtcQuery(to)}");
        if (!string.IsNullOrWhiteSpace(failureCode)) parts.Add($"failureCode={Uri.EscapeDataString(failureCode.Trim())}");
        return SendJsonAsync<DataSetValidationExecutionPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"datasets/validations?{string.Join("&", parts)}"), tenantId), cancellationToken);
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

    /// <summary>검증 결과 상세를 0 이상인 건너뛸 개수와 1~500 범위의 페이지 크기로 조회합니다.</summary>
    /// <param name="tenantId">검증 실행이 속한 Tenant 식별자입니다.</param>
    /// <param name="executionId">상세 결과를 조회할 검증 실행 식별자입니다.</param>
    /// <param name="skip">앞에서 건너뛸 상세 결과 개수입니다. 0 이상이어야 합니다.</param>
    /// <param name="take">반환할 상세 결과 개수입니다. 1 이상 500 이하여야 합니다.</param>
    /// <param name="cancellationToken">요청 취소를 감시하는 토큰입니다.</param>
    /// <exception cref="ArgumentOutOfRangeException">skip 또는 take가 허용 범위를 벗어나면 발생합니다.</exception>
    /// <exception cref="TensorApiException">검증 실행이 없거나 API가 상세 조회를 거부하면 발생합니다.</exception>
    public Task<DataSetValidationDetailsPageContract> GetDataSetValidationDetailsAsync(
        Guid tenantId, Guid executionId, int skip = 0, int take = 100,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(executionId, nameof(executionId));
        if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
        if (take is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(take));
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

    public Task<WorkerTaskStatusPageContract> GetWorkerTasksAsync(
        Guid tenantId, string? taskType = null, string? status = null,
        DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(taskType)) parts.Add($"taskType={Uri.EscapeDataString(taskType.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={UtcQuery(from)}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={UtcQuery(to)}");
        return SendJsonAsync<WorkerTaskStatusPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"worker/tasks?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<WorkerTaskResultContract?> GetWorkerTaskResultAsync(Guid tenantId, Guid taskId,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidateId(taskId, nameof(taskId));
        return SendOptionalAsync<WorkerTaskResultContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"worker/tasks/{taskId:D}/result"), tenantId), cancellationToken);
    }

    public Task CancelWorkerTaskAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidateId(taskId, nameof(taskId));
        return SendNoContentAsync(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"worker/tasks/{taskId:D}/cancel"), tenantId), cancellationToken);
    }

    public Task RetryWorkerTaskAsync(Guid tenantId, Guid taskId, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId); ValidateId(taskId, nameof(taskId));
        return SendNoContentAsync(CreateRequest(HttpMethod.Post,
            TenantPath(tenantId, $"worker/tasks/{taskId:D}/retry"), tenantId), cancellationToken);
    }

    /// <summary>Tenant 관리자가 조회할 DLQ 작업을 1 이상인 페이지와 1~100 범위의 페이지 크기로 반환합니다.</summary>
    /// <param name="tenantId">DLQ 작업이 속한 Tenant 식별자입니다.</param>
    /// <param name="page">조회할 페이지 번호입니다. 1 이상이어야 하며 페이지 오프셋이 정수 범위를 넘지 않아야 합니다.</param>
    /// <param name="pageSize">페이지에 반환할 작업 수입니다. 1 이상 100 이하여야 합니다.</param>
    /// <param name="cancellationToken">요청 취소를 감시하는 토큰입니다.</param>
    /// <exception cref="ArgumentOutOfRangeException">page 또는 pageSize가 허용 범위를 벗어나거나 페이지 오프셋이 정수 범위를 넘으면 발생합니다.</exception>
    /// <exception cref="TensorApiException">Tenant 관리자가 아니거나 API가 DLQ 조회를 거부하면 발생합니다.</exception>
    public Task<WorkerDeadLetterPageContract> GetWorkerDeadLettersAsync(
        Guid tenantId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        if ((long)(page - 1) * pageSize > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(page), "페이지 오프셋이 정수 범위를 초과합니다.");
        return SendJsonAsync<WorkerDeadLetterPageContract>(
            CreateRequest(HttpMethod.Get, TenantPath(tenantId, $"worker/dead-letters?page={page}&pageSize={pageSize}"), tenantId),
            cancellationToken);
    }

    public Task<WorkerDeadLetterPageContract> GetWorkerDeadLettersFilteredAsync(
        Guid tenantId, string? taskType = null, string? errorCode = null,
        DateTimeOffset? fromUtc = null, DateTimeOffset? toUtc = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(taskType)) parts.Add($"taskType={Uri.EscapeDataString(taskType.Trim())}");
        if (!string.IsNullOrWhiteSpace(errorCode)) parts.Add($"errorCode={Uri.EscapeDataString(errorCode.Trim())}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={UtcQuery(from)}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={UtcQuery(to)}");
        return SendJsonAsync<WorkerDeadLetterPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"worker/dead-letters?{string.Join("&", parts)}"), tenantId), cancellationToken);
    }

    public Task<WorkerReplayHistoryPageContract> GetWorkerReplayHistoryAsync(
        Guid tenantId, Guid? originalTaskId = null, DateTimeOffset? fromUtc = null,
        DateTimeOffset? toUtc = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantId(tenantId);
        ValidatePage(page, pageSize);
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (originalTaskId is Guid original) parts.Add($"originalTaskId={original:D}");
        if (fromUtc is DateTimeOffset from) parts.Add($"fromUtc={UtcQuery(from)}");
        if (toUtc is DateTimeOffset to) parts.Add($"toUtc={UtcQuery(to)}");
        return SendJsonAsync<WorkerReplayHistoryPageContract>(CreateRequest(HttpMethod.Get,
            TenantPath(tenantId, $"worker/dead-letters/replays?{string.Join("&", parts)}"), tenantId), cancellationToken);
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

    private HttpRequestMessage SendRequest(HttpMethod method, string path, Guid tenantId, object content)
    {
        var request = CreateRequest(method, path, tenantId);
        request.Content = JsonContent.Create(content, options: JsonOptions);
        return request;
    }

    private Task UpdateDirectoryEntryAsync(Guid tenantId, string suffix,
        UpdateDirectoryEntryRequestContract request, CancellationToken cancellationToken)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        var path = string.IsNullOrWhiteSpace(suffix)
            ? $"/api/platform/tenants/{tenantId:D}"
            : TenantPath(tenantId, suffix);
        return SendNoContentAsync(SendRequest(HttpMethod.Put, path, tenantId, request), cancellationToken);
    }

    private Task UpdateMembershipAsync(Guid tenantId, string suffix, UpdateMembershipRequestContract request,
        CancellationToken cancellationToken)
    {
        ValidateTenantId(tenantId);
        ArgumentNullException.ThrowIfNull(request);
        return SendNoContentAsync(SendRequest(HttpMethod.Patch, TenantPath(tenantId, suffix), tenantId, request), cancellationToken);
    }

    private static string ValidateAndFormatId(Guid value, string paramName)
    {
        ValidateId(value, paramName);
        return value.ToString("D");
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

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));
        if ((long)(page - 1) * pageSize > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(page), "페이지 오프셋이 정수 범위를 초과합니다.");
    }

    private static void ValidateDateWindow(DateTimeOffset fromUtc, DateTimeOffset toUtc)
    {
        if (fromUtc > toUtc) throw new ArgumentException("시작 시각은 종료 시각보다 늦을 수 없습니다.");
    }

    private static string UtcQuery(DateTimeOffset value) =>
        Uri.EscapeDataString(value.ToUniversalTime().ToString("O"));

    private static string Query(string? search, string? status, int page, int pageSize,
        (string Key, string Value)? extra = null)
    {
        var parts = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(search))
            parts.Add($"search={Uri.EscapeDataString(search.Trim())}");
        if (!string.IsNullOrWhiteSpace(status))
            parts.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (extra is { } value)
            parts.Add($"{value.Key}={Uri.EscapeDataString(value.Value)}");
        return "?" + string.Join("&", parts);
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

