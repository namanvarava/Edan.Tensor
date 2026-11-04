using EDAN.Tensor.Platform.Application.UseCases;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.SharedKernel.Results;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>플랫폼 관리 API는 응용 서비스의 업무 결과를 HTTP 응답으로 변환합니다.</summary>
public static class PlatformEndpoints
{
    /// <summary>테넌트·조직·프로젝트·사용자·자원·정책 생성 API를 등록합니다.</summary>
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/platform").WithTags("Platform");

        group.MapPost("/tenants", async (CreateTenantRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateTenantAsync(request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

        group.MapPost("/tenants/{tenantId:guid}/activate", async (Guid tenantId, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.ActivateTenantAsync(new TenantId(tenantId), token);
            return result.IsSuccess ? Results.NoContent() : ToHttpResult(result);
        });

        group.MapPost("/tenants/{tenantId:guid}/organizations", async (Guid tenantId, CreateOrganizationRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateOrganizationAsync(new TenantId(tenantId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

        group.MapPost("/tenants/{tenantId:guid}/organizations/{organizationId:guid}/projects", async (Guid tenantId, Guid organizationId, CreateProjectRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.CreateProjectAsync(new TenantId(tenantId), new OrganizationId(organizationId), request.Code, request.Name, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

        group.MapPost("/tenants/{tenantId:guid}/users/invitations", async (Guid tenantId, InviteUserRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            var result = await service.InviteUserAsync(new TenantId(tenantId), request.Email, request.DisplayName, token);
            return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
        });

        group.MapPost("/resources", async (CreateResourceRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            try
            {
                var scope = new ResourceScope(new TenantId(request.TenantId),
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null);
                var result = await service.CreateResourceAsync(scope, request.ResourceType, request.Name, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }
            catch (ArgumentException ex) { return ValidationError(ex.Message); }
        });

        group.MapPost("/policies", async (CreatePolicyRequest request, PlatformProvisioningService service, CancellationToken token) =>
        {
            try
            {
                var scope = new ResourceScope(new TenantId(request.TenantId),
                    request.OrganizationId is Guid organizationId ? new OrganizationId(organizationId) : null,
                    request.ProjectId is Guid projectId ? new ProjectId(projectId) : null);
                var result = await service.CreatePolicyAsync(scope, request.Key, request.Value, token);
                return result.IsSuccess ? Results.Json(new { Id = result.Value.Value }, statusCode: StatusCodes.Status201Created) : ToHttpResult(result);
            }
            catch (ArgumentException ex) { return ValidationError(ex.Message); }
        });

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
public sealed record CreateOrganizationRequest(string Code, string Name);
public sealed record CreateProjectRequest(string Code, string Name);
public sealed record InviteUserRequest(string Email, string DisplayName);
public sealed record CreateResourceRequest(Guid TenantId, Guid? OrganizationId, Guid? ProjectId, string ResourceType, string Name);
public sealed record CreatePolicyRequest(Guid TenantId, Guid? OrganizationId, Guid? ProjectId, string Key, string Value);
