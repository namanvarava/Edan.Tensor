using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.ApiHost.Services;
using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>플랫폼 관리 API는 인증된 주체의 Tenant 권한을 기준으로 업무를 수행합니다.</summary>
public static class PlatformEndpoints
{
    /// <summary>플랫폼 관리 API를 관리자 및 Tenant 구성원 경계로 나누어 등록합니다.</summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/platform").WithTags("Platform");

        // Tenant 생성과 최초 활성화는 플랫폼 관리자에게만 허용합니다.
        group.MapPost("/tenants", async (CreateTenantRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateTenantAsync(request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).RequireAuthorization("PlatformAdministrator");

        group.MapPost("/tenants/{tenantId:guid}/memberships", async (Guid tenantId, GrantTenantMembershipRequest request, TenantMembershipService service, CancellationToken token) =>
        {
            if (!Enum.TryParse<TenantRole>(request.Role, true, out var role))
                return ValidationError("지원하지 않는 테넌트 역할입니다.");
            var result = await service.GrantAsync(new TenantId(tenantId), request.SubjectId, role, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).RequireAuthorization("PlatformAdministrator");

        group.MapPost("/tenants/{tenantId:guid}/activate", async (Guid tenantId, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.ActivateTenantAsync(new TenantId(tenantId), token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        }).RequireAuthorization("PlatformAdministrator");

        group.MapGet("/tenants/mine", async (HttpContext context, TenantMembershipService service, CancellationToken token) =>
        {
            var subjectId = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(subjectId))
                return Results.Unauthorized();
            var memberships = await service.GetMyMembershipsAsync(subjectId, token);
            return Results.Ok(memberships.Select(x => new { TenantId = x.TenantId.Value, Role = x.Role.ToString() }));
        }).RequireAuthorization("TenantMember");

        // 경로의 TenantId가 요청에서 선택한 Tenant와 일치하고 활성 소속이 확인된 경우에만 업무 API를 실행합니다.
        var tenantGroup = group.MapGroup("/tenants")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-policies",
            async (Guid tenantId, CreateAuthorizationPolicyRequest request, AuthorizationPolicyService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<AuthorizationPolicyEffect>(request.Effect, true, out var effect))
                    return ValidationError("지원하지 않는 접근 정책 효과입니다.");
                var result = await service.CreateAsync(new TenantId(tenantId),
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null,
                    request.SubjectId, request.Action, request.ResourceType, effect, request.Priority, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/authorization-policies",
            async (Guid tenantId, AuthorizationPolicyService service, CancellationToken token) =>
            {
                var policies = await service.GetByTenantAsync(new TenantId(tenantId), token);
                return Results.Ok(policies.Select(x => new
                {
                    Id = x.Id.Value,
                    OrganizationId = x.OrganizationId?.Value,
                    ProjectId = x.ProjectId?.Value,
                    x.SubjectId,
                    x.Action,
                    x.ResourceType,
                    Effect = x.Effect.ToString(),
                    x.Priority,
                    x.IsEnabled,
                    x.CreatedAtUtc
                }));
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 권한 감사 조회는 Tenant 관리자에게만 허용하며 감사 데이터는 읽기 전용으로 제공합니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit",
            async (Guid tenantId, AuthorizationAuditQueryService service, CancellationToken token,
                string? subjectId, string? action, bool? isAllowed, DateTimeOffset? fromUtc,
                DateTimeOffset? toUtc, int page = 1, int pageSize = 50) =>
            {
                try
                {
                    var result = await service.GetPageAsync(new TenantId(tenantId), subjectId, action,
                        isAllowed, fromUtc, toUtc, page, pageSize, token);
                    return Results.Ok(new
                    {
                        result.TotalCount,
                        result.Page,
                        result.PageSize,
                        Items = result.Items.Select(x => new
                        {
                            Id = x.Id.Value,
                            x.SubjectId,
                            x.Action,
                            x.ResourceType,
                            x.Scope,
                            x.IsAllowed,
                            x.ReasonCode,
                            x.MatchedPolicyId,
                            x.OccurredAtUtc
                        })
                    });
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 감사 무결성 체크섬은 별도 보관 후 검증 요청에 재사용합니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/integrity",
            async (Guid tenantId, AuthorizationAuditIntegrityService service, CancellationToken token,
                DateTimeOffset fromUtc, DateTimeOffset toUtc) =>
            {
                try
                {
                    var result = await service.CalculateAsync(new TenantId(tenantId), fromUtc, toUtc, token);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/integrity/verify",
            async (Guid tenantId, VerifyAuthorizationAuditIntegrityRequest request,
                AuthorizationAuditIntegrityService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.VerifyAsync(new TenantId(tenantId), request.FromUtc,
                        request.ToUtc, request.ExpectedSha256, token);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 감사 아카이브는 Tenant별로 생성하며 검증 전 원본 기록은 변경하지 않습니다.
        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archives",
            async (Guid tenantId, CreateAuthorizationAuditArchiveRequest request,
                AuthorizationAuditArchiveService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.CreateAsync(new TenantId(tenantId),
                        request.FromUtc, request.ToUtc, token);
                    return Results.Json(result, statusCode: StatusCodes.Status201Created);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archives/{archiveId:guid}/verify",
            async (Guid tenantId, Guid archiveId, VerifyAuthorizationAuditArchiveRequest request,
                AuthorizationAuditArchiveService service, CancellationToken token) =>
            {
                var result = await service.VerifyAsync(new TenantId(tenantId), archiveId, request.ExpectedSha256, token);
                if (result.ReasonCode == "archive.not_found")
                    return Results.NotFound(result);
                return Results.Ok(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 아카이브 작업의 상태와 재시도·검증 이력을 Tenant 관리자에게 제공합니다.
        // Tenant 관리자는 아카이브 예약 주기와 활성 상태를 관리합니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/archive-schedule",
            async (Guid tenantId, AuthorizationAuditArchiveScheduleService service, CancellationToken token) =>
                Results.Ok(await service.GetAsync(new TenantId(tenantId), token)))
            .AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/authorization-audit/archive-schedule",
            async (Guid tenantId, UpdateAuthorizationAuditArchiveScheduleRequest request,
                AuthorizationAuditArchiveScheduleService service, CancellationToken token) =>
            {
                try
                {
                    return Results.Ok(await service.UpdateAsync(new TenantId(tenantId),
                        request.IntervalHours, request.IsEnabled, token));
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archive-jobs",
            async (Guid tenantId, CreateAuthorizationAuditArchiveRequest request, HttpContext context,
                AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var result = await service.CreateAsync(new TenantId(tenantId), request.FromUtc,
                        request.ToUtc, actor, token);
                    return Results.Json(result, statusCode: StatusCodes.Status201Created);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/archive-jobs/{jobId:guid}",
            async (Guid tenantId, Guid jobId, AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                var result = await service.GetAsync(new TenantId(tenantId), jobId, token);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archive-jobs/{jobId:guid}/retry",
            async (Guid tenantId, Guid jobId, AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.RetryAsync(new TenantId(tenantId), jobId, token);
                    return result is null ? Results.NotFound() : Results.Ok(result);
                }
                catch (InvalidOperationException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/authorization-audit/archive-jobs/{jobId:guid}/verify",
            async (Guid tenantId, Guid jobId, VerifyAuthorizationAuditArchiveRequest request,
                AuthorizationAuditArchiveJobService service, CancellationToken token) =>
            {
                try
                {
                    var result = await service.VerifyAsync(new TenantId(tenantId), jobId,
                        request.ExpectedSha256, token);
                    return result is null ? Results.NotFound() : Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
                catch (InvalidOperationException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 감사 보존 정책은 Tenant 관리자만 변경하며 모든 변경은 별도 감사 이력으로 남깁니다.
        tenantGroup.MapGet("/{tenantId:guid}/authorization-audit/retention",
            async (Guid tenantId, AuthorizationAuditRetentionService service, CancellationToken token) =>
            {
                var result = await service.GetAsync(new TenantId(tenantId), token);
                return Results.Ok(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/authorization-audit/retention",
            async (Guid tenantId, UpdateAuthorizationAuditRetentionRequest request, HttpContext context,
                AuthorizationAuditRetentionService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                try
                {
                    var result = await service.UpdateAsync(new TenantId(tenantId), request.RetentionDays,
                        request.IsLegalHold, request.LegalHoldReason, actor, token);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex) { return ValidationError(ex.Message); }
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPut("/{tenantId:guid}/authorization-policies/{policyId:guid}",
            async (Guid tenantId, Guid policyId, UpdateAuthorizationPolicyRequest request, HttpContext context,
                AuthorizationPolicyService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<AuthorizationPolicyEffect>(request.Effect, true, out var effect))
                    return ValidationError("지원하지 않는 접근 정책 효과입니다.");
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                var result = await service.UpdateAsync(new TenantId(tenantId), new AuthorizationPolicyId(policyId),
                    effect, request.Priority, actor, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPatch("/{tenantId:guid}/authorization-policies/{policyId:guid}/enabled",
            async (Guid tenantId, Guid policyId, SetAuthorizationPolicyEnabledRequest request, HttpContext context,
                AuthorizationPolicyService service, CancellationToken token) =>
            {
                var actor = context.User.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(actor)) return Results.Unauthorized();
                var result = await service.SetEnabledAsync(new TenantId(tenantId), new AuthorizationPolicyId(policyId),
                    request.IsEnabled, actor, token);
                return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/memberships",
            async (Guid tenantId, Guid organizationId, GrantOrganizationMembershipRequest request, ScopedMembershipService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<OrganizationRole>(request.Role, true, out var role))
                    return ValidationError("지원하지 않는 조직 역할입니다.");
                var result = await service.GrantOrganizationAsync(new TenantId(tenantId), new OrganizationId(organizationId), request.SubjectId, role, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/projects/{projectId:guid}/memberships",
            async (Guid tenantId, Guid organizationId, Guid projectId, GrantProjectMembershipRequest request, ScopedMembershipService service, CancellationToken token) =>
            {
                if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role))
                    return ValidationError("지원하지 않는 프로젝트 역할입니다.");
                var result = await service.GrantProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), new ProjectId(projectId), request.SubjectId, role, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 조직 생성은 Tenant 관리자만 수행하여 일반 구성원의 Tenant 구조 변경을 차단합니다.
        tenantGroup.MapPost("/{tenantId:guid}/organizations", async (Guid tenantId, CreateOrganizationRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateOrganizationAsync(new TenantId(tenantId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 프로젝트 생성은 Tenant 관리자만 수행하여 조직 하위 구조 변경을 제한합니다.
        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/projects", async (Guid tenantId, Guid organizationId, CreateProjectRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 사용자 초대는 Tenant 관리자만 수행하여 외부 주체 연결을 임의로 생성하지 못하게 합니다.
        tenantGroup.MapPost("/{tenantId:guid}/users/invitations", async (Guid tenantId, InviteUserRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.InviteUserAsync(new TenantId(tenantId), request.Email, request.DisplayName, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        // 자원과 정책의 Tenant 범위는 요청 본문이 아닌 선택된 Tenant 헤더와 데이터베이스 소속 검증으로 결정합니다.
        group.MapPost("/resources", async (CreateResourceRequest request, ITenantContext tenantContext, PlatformProvisioningService service, CancellationToken token) =>
        {
            try
            {
                var scope = new ResourceScope(tenantContext.TenantId,
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null);
                var result = await service.CreateResourceAsync(scope, request.ResourceType, request.Name, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }
            catch (ArgumentException ex) { return ValidationError(ex.Message); }
        }).RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>().AddEndpointFilter<ScopedResourceAuthorizationFilter>();

        group.MapPost("/policies", async (CreatePolicyRequest request, ITenantContext tenantContext, PlatformProvisioningService service, CancellationToken token) =>
        {
            try
            {
                var scope = new ResourceScope(tenantContext.TenantId,
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null);
                var result = await service.CreatePolicyAsync(scope, request.Key, request.Value, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }
            catch (ArgumentException ex) { return ValidationError(ex.Message); }
        }).RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>().AddEndpointFilter<ScopedResourceAuthorizationFilter>();

        return endpoints;
    }

    /// <summary>업무 오류 코드를 표준 HTTP 상태 코드로 변환합니다.</summary>
    private static IResult ToHttpResult(Result result)
    {
        var code = result.Error.Code;
        var status = code.EndsWith(".not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound
            : code.Contains("conflict", StringComparison.Ordinal) ? StatusCodes.Status409Conflict
            : code.Contains("unavailable", StringComparison.Ordinal) || code.Contains("scope_mismatch", StringComparison.Ordinal) || code.Contains("tenant_required", StringComparison.Ordinal)
                ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return Results.Json(new ApiError(code, result.Error.Description), statusCode: status);
    }

    private static IResult ValidationError(string description) =>
        Results.Json(new ApiError("platform.validation", description), statusCode: StatusCodes.Status400BadRequest);
}

public sealed record ApiError(string Code, string Description);
public sealed record CreateTenantRequest(string Code, string Name);
public sealed record GrantTenantMembershipRequest(string SubjectId, string Role);
public sealed record GrantOrganizationMembershipRequest(string SubjectId, string Role);
public sealed record GrantProjectMembershipRequest(string SubjectId, string Role);
public sealed record CreateAuthorizationPolicyRequest(Guid? OrganizationId, Guid? ProjectId, string SubjectId, string Action, string ResourceType, string Effect, int Priority);
public sealed record UpdateAuthorizationPolicyRequest(string Effect, int Priority);
public sealed record SetAuthorizationPolicyEnabledRequest(bool IsEnabled);
public sealed record VerifyAuthorizationAuditIntegrityRequest(DateTimeOffset FromUtc, DateTimeOffset ToUtc, string ExpectedSha256);
public sealed record CreateAuthorizationAuditArchiveRequest(DateTimeOffset FromUtc, DateTimeOffset ToUtc);
public sealed record VerifyAuthorizationAuditArchiveRequest(string? ExpectedSha256);
public sealed record UpdateAuthorizationAuditRetentionRequest(int RetentionDays, bool IsLegalHold, string? LegalHoldReason);
public sealed record UpdateAuthorizationAuditArchiveScheduleRequest(int IntervalHours, bool IsEnabled);
public sealed record CreateOrganizationRequest(string Code, string Name);
public sealed record CreateProjectRequest(string Code, string Name);
public sealed record InviteUserRequest(string Email, string DisplayName);
public sealed record CreateResourceRequest(Guid? OrganizationId, Guid? ProjectId, string ResourceType, string Name);
public sealed record CreatePolicyRequest(Guid? OrganizationId, Guid? ProjectId, string Key, string Value);

public sealed record CreateStorageQuotaPolicyRequest(string Scope, Guid TargetId, long? TenantLimitBytes,
    long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes,
    int WarningThresholdPercent = 80, string? PlanCode = "Community");
public sealed record UpdateStorageQuotaPolicyRequest(long ExpectedVersion, long? TenantLimitBytes,
    long? OrganizationLimitBytes, long? ProjectLimitBytes, long? DataSetLimitBytes,
    int WarningThresholdPercent, string PlanCode);
public sealed record SetStorageQuotaPolicyEnabledRequest(bool IsEnabled, long ExpectedVersion);
