using EDAN.Tensor.Business.Application;
using EDAN.Tensor.Business.Contracts;
using EDAN.Tensor.Master.Application;
using EDAN.Tensor.Master.Contracts;
using EDAN.Tensor.System.Application;
using EDAN.Tensor.System.Contracts;
using EDAN.Tensor.ApiHost.Security;

namespace EDAN.Tensor.ApiHost.Endpoints;

public static class ModuleV1Endpoints
{
    public static void MapModuleV1Endpoints(this IEndpointRouteBuilder endpoints)
    {
        var system = endpoints.MapGroup("/api/v1/system").WithTags("System v1").RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>();
        var listMenus = system.MapGet("/menus", (Guid tenantId, HttpRequest request, ISystemMenuService service) =>
            TenantResponse(tenantId, request, () => Results.Ok(new SystemMenuPageContract(
                service.List(tenantId, request.Query["search"], ParseBool(request.Query["isEnabled"]), ParseInt(request.Query["page"], 1), ParseInt(request.Query["pageSize"], 50), request.Query["sort"]), 0, ParseInt(request.Query["page"], 1), ParseInt(request.Query["pageSize"], 50)))));
        listMenus.WithName("ListSystemMenusV1").WithOpenApi(operation => { operation.Summary = "시스템 메뉴 조회"; return operation; });
        var createMenu = system.MapPost("/menus", (Guid tenantId, UpsertSystemMenuRequest request, HttpRequest httpRequest, ISystemMenuService service) =>
            TenantResponse(tenantId, httpRequest, () => Results.Created($"/api/v1/system/menus?tenantId={tenantId}", service.Create(tenantId, request))));
        createMenu.WithName("CreateSystemMenuV1").WithOpenApi(operation => { operation.Summary = "시스템 메뉴 등록"; return operation; });

        var master = endpoints.MapGroup("/api/v1/master").WithTags("Master v1").RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>();
        var listCodes = master.MapGet("/codes", (Guid tenantId, HttpRequest request, IMasterCodeService service) =>
            TenantResponse(tenantId, request, () => Results.Ok(new MasterCodePageContract(
                service.List(tenantId, request.Query["groupCode"], request.Query["search"], ParseBool(request.Query["isEnabled"]), ParseInt(request.Query["page"], 1), ParseInt(request.Query["pageSize"], 50), request.Query["sort"]), 0, ParseInt(request.Query["page"], 1), ParseInt(request.Query["pageSize"], 50)))));
        listCodes.WithName("ListMasterCodesV1").WithOpenApi(operation => { operation.Summary = "공통코드 조회"; return operation; });
        var createCode = master.MapPost("/codes", (Guid tenantId, UpsertMasterCodeRequest request, HttpRequest httpRequest, IMasterCodeService service) =>
            TenantResponse(tenantId, httpRequest, () => Results.Created($"/api/v1/master/codes?tenantId={tenantId}", service.Create(tenantId, request))));
        createCode.WithName("CreateMasterCodeV1").WithOpenApi(operation => { operation.Summary = "공통코드 등록"; return operation; });

        var business = endpoints.MapGroup("/api/v1/business").WithTags("Business v1").RequireAuthorization("TenantMember").AddEndpointFilter<TenantMembershipAuthorizationFilter>();
        var listWorkItems = business.MapGet("/work-items", (Guid tenantId, HttpRequest request, IBusinessWorkItemService service) =>
            TenantResponse(tenantId, request, () => Results.Ok(new BusinessWorkItemPageContract(
                service.List(tenantId, request.Query["workType"], request.Query["status"], ParseInt(request.Query["page"], 1), ParseInt(request.Query["pageSize"], 50), request.Query["sort"]), 0, ParseInt(request.Query["page"], 1), ParseInt(request.Query["pageSize"], 50)))));
        listWorkItems.WithName("ListBusinessWorkItemsV1").WithOpenApi(operation => { operation.Summary = "업무 처리 항목 조회"; return operation; });
        var createWorkItem = business.MapPost("/work-items", (Guid tenantId, CreateBusinessWorkItemRequest request, HttpRequest httpRequest, IBusinessWorkItemService service) =>
            TenantResponse(tenantId, httpRequest, () => Results.Created($"/api/v1/business/work-items?tenantId={tenantId}", service.Create(tenantId, request))));
        createWorkItem.WithName("CreateBusinessWorkItemV1").WithOpenApi(operation => { operation.Summary = "업무 처리 항목 등록"; return operation; });
    }

    private static IResult TenantResponse(Guid tenantId, HttpRequest request, Func<IResult> action)
    {
        if (!Guid.TryParse(request.Headers["X-Tenant-Id"], out var headerTenant) || headerTenant != tenantId)
            return Results.Problem("X-Tenant-Id가 경로의 Tenant와 일치하지 않습니다.", statusCode: StatusCodes.Status400BadRequest, extensions: new Dictionary<string, object?> { ["code"] = "tenant.context_mismatch" });
        return action();
    }

    private static int ParseInt(string? value, int fallback) => int.TryParse(value, out var parsed) ? Math.Clamp(parsed, 1, 200) : fallback;
    private static bool? ParseBool(string? value) => bool.TryParse(value, out var parsed) ? parsed : null;
}
