using EDAN.Tensor.ApiHost.Security;
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

        // 경로의 TenantId가 요청에서 선택한 Tenant와 일치하고 활성 소속이 확인된 경우에만 업무 API를 실행합니다.
        var tenantGroup = group.MapGroup("/tenants")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        tenantGroup.MapPost("/{tenantId:guid}/organizations", async (Guid tenantId, CreateOrganizationRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateOrganizationAsync(new TenantId(tenantId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

        tenantGroup.MapPost("/{tenantId:guid}/organizations/{organizationId:guid}/projects", async (Guid tenantId, Guid organizationId, CreateProjectRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

        tenantGroup.MapPost("/{tenantId:guid}/users/invitations", async (Guid tenantId, InviteUserRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.InviteUserAsync(new TenantId(tenantId), request.Email, request.DisplayName, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

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
        }).RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>();

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
        }).RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        return endpoints;
    }

    /// <summary>업무 오류 코드를 표준 HTTP 상태 코드로 변환합니다.</summary>
    private static IResult ToHttpResult(Result result)
    {
        var code = result.Error.Code;
        var status = code.EndsWith(".not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound
            : code.Contains("conflict", StringComparison.Ordinal) ? StatusCodes.Status409Conflict
            : code.Contains("unavailable", StringComparison.Ordinal) || code.Contains("scope_mismatch", StringComparison.Ordinal)
                ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest;
        return Results.Json(new ApiError(code, result.Error.Description), statusCode: status);
    }

    private static IResult ValidationError(string description) =>
        Results.Json(new ApiError("platform.validation", description), statusCode: StatusCodes.Status400BadRequest);
}

public sealed record ApiError(string Code, string Description);
public sealed record CreateTenantRequest(string Code, string Name);
public sealed record GrantTenantMembershipRequest(string SubjectId, string Role);
public sealed record CreateOrganizationRequest(string Code, string Name);
public sealed record CreateProjectRequest(string Code, string Name);
public sealed record InviteUserRequest(string Email, string DisplayName);
public sealed record CreateResourceRequest(Guid? OrganizationId, Guid? ProjectId, string ResourceType, string Name);
public sealed record CreatePolicyRequest(Guid? OrganizationId, Guid? ProjectId, string Key, string Value);
