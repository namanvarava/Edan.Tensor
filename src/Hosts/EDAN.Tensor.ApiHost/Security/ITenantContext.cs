using EDAN.Tensor.ApiHost.Endpoints;
using EDAN.Tensor.Platform.Application.Abstractions;
using EDAN.Tensor.Platform.Domain.Model;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>현재 요청에서 선택한 테넌트 범위를 제공합니다.</summary>
public interface ITenantContext
{
    TenantId TenantId { get; }
}

/// <summary>요청 헤더의 Tenant 선택값을 읽으며 권한은 별도 소속 저장소에서 검증합니다.</summary>
public sealed class RequestTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    public TenantId TenantId
    {
        get
        {
            var value = accessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (!Guid.TryParse(value, out var tenantId) || tenantId == Guid.Empty)
                throw new InvalidOperationException("유효한 X-Tenant-Id 선택값이 필요합니다.");
            return new TenantId(tenantId);
        }
    }
}

/// <summary>경로의 Tenant와 요청에서 선택한 Tenant가 일치하는지 검사합니다.</summary>
public sealed class TenantRouteAuthorizationFilter(ITenantContext tenantContext) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            if (!context.HttpContext.Request.RouteValues.TryGetValue("tenantId", out var routeValue)
                || !Guid.TryParse(routeValue?.ToString(), out var routeTenantId)
                || routeTenantId != tenantContext.TenantId.Value)
                return Forbidden();
        }
        catch (InvalidOperationException) { return Forbidden(); }

        return await next(context);
    }

    private static IResult Forbidden() =>
        Results.Json(new ApiError("platform.tenant.forbidden", "선택한 테넌트에 접근할 권한이 없습니다."),
            statusCode: StatusCodes.Status403Forbidden);
}

/// <summary>외부 인증 주체가 선택한 테넌트의 활성 구성원인지 데이터베이스에서 확인합니다.</summary>
public sealed class TenantMembershipAuthorizationFilter(
    ITenantContext tenantContext,
    ITenantMembershipRepository memberships,
    IHttpContextAccessor accessor) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var subjectId = accessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(subjectId))
            return Forbidden();

        try
        {
            if (!await memberships.HasActiveMembershipAsync(tenantContext.TenantId, subjectId,
                    context.HttpContext.RequestAborted))
                return Forbidden();
        }
        catch (InvalidOperationException) { return Forbidden(); }

        return await next(context);
    }

    private static IResult Forbidden() =>
        Results.Json(new ApiError("platform.membership.forbidden", "선택한 테넌트의 활성 구성원이 아닙니다."),
            statusCode: StatusCodes.Status403Forbidden);
}
