using System.Security.Claims;
using EDAN.Tensor.Platform.Domain.Model;
using EDAN.Tensor.ApiHost.Endpoints;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>현재 인증 주체에 연결된 테넌트 범위를 제공합니다.</summary>
public interface ITenantContext
{
    TenantId TenantId { get; }
}

/// <summary>인증 클레임에서 현재 테넌트 식별자를 안전하게 읽습니다.</summary>
public sealed class ClaimsTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    public TenantId TenantId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue("tenant_id");
            if (!Guid.TryParse(value, out var tenantId) || tenantId == Guid.Empty)
                throw new InvalidOperationException("인증 주체에 유효한 Tenant 식별자가 없습니다.");
            return new TenantId(tenantId);
        }
    }
}

/// <summary>경로에 지정된 테넌트가 인증 주체의 테넌트와 일치하는지 검사합니다.</summary>
public sealed class TenantRouteAuthorizationFilter(ITenantContext tenantContext) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.RouteValues.TryGetValue("tenantId", out var routeValue)
            || !Guid.TryParse(routeValue?.ToString(), out var routeTenantId)
            || routeTenantId != tenantContext.TenantId.Value)
        {
            return Results.Json(new ApiError("platform.tenant.forbidden", "요청한 테넌트에 접근할 권한이 없습니다."),
                statusCode: StatusCodes.Status403Forbidden);
        }

        return await next(context);
    }
}
