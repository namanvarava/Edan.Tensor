using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.DataSet.Application.UseCases;
using EDAN.Tensor.DataSet.Domain.Model;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>DataSet 메타데이터 관리 API를 Tenant 소유권과 관리자 권한 경계로 노출합니다.</summary>
public static class DataSetEndpoints
{
    public static IEndpointRouteBuilder MapDataSetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var tenantGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/datasets")
            .WithTags("DataSet")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        tenantGroup.MapPost("", async (Guid tenantId, CreateDataSetRequest request,
            DataSetManagementService service, CancellationToken token) =>
        {
            try
            {
                var scope = new DataSetScope(tenantId, request.OrganizationId, request.ProjectId);
                var result = await service.CreateAsync(scope, request.Name, request.StorageAdapter, token);
                if (!result.IsSuccess)
                    return Results.BadRequest(new { result.Error.Code, result.Error.Description });

                var id = result.Value.Value;
                return Results.Created($"/api/platform/tenants/{tenantId}/datasets/{id}",
                    new { Id = id, TenantId = tenantId, request.OrganizationId, request.ProjectId, request.Name, request.StorageAdapter, Status = "Draft" });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Code = "dataset.invalid_request", Description = ex.Message });
            }
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("CreateDataSet");

        tenantGroup.MapGet("/{dataSetId:guid}", async (Guid tenantId, Guid dataSetId,
            DataSetManagementService service, CancellationToken token) =>
        {
            var dataSet = await service.GetAsync(new DataSetId(dataSetId), tenantId, token);
            return dataSet is null
                ? Results.NotFound(new { Code = "dataset.not_found", Description = "Tenant 범위에서 DataSet을 찾을 수 없습니다." })
                : Results.Ok(new
                {
                    Id = dataSet.Id.Value,
                    TenantId = dataSet.TenantId,
                    OrganizationId = dataSet.OrganizationId,
                    ProjectId = dataSet.ProjectId,
                    dataSet.Name,
                    dataSet.StorageAdapter,
                    Status = dataSet.Status.ToString(),
                    dataSet.Version,
                    dataSet.CreatedAtUtc,
                    dataSet.CurrentSchemaVersion,
                    SchemaRevisions = dataSet.SchemaRevisions.Select(revision => new
                    {
                        revision.Version,
                        revision.SchemaJson,
                        revision.CreatedAtUtc
                    })
                });
        }).WithName("GetDataSet");

        tenantGroup.MapPost("/{dataSetId:guid}/schemas", async (Guid tenantId, Guid dataSetId,
            AddDataSetSchemaRequest request, DataSetManagementService service, ILoggerFactory loggerFactory,
            IWebHostEnvironment environment, CancellationToken token) =>
        {
            try
            {
                var result = await service.AddSchemaAsync(new DataSetId(dataSetId), tenantId,
                    request.SchemaJson, DateTimeOffset.UtcNow, token);
                return result.IsSuccess
                    ? Results.Created($"/api/platform/tenants/{tenantId}/datasets/{dataSetId}",
                        new { DataSetId = dataSetId, SchemaVersion = result.Value })
                    : Results.BadRequest(new { result.Error.Code, result.Error.Description });
            }
            catch (Exception exception)
            {
                loggerFactory.CreateLogger("DataSetEndpoints").LogError(exception, "DataSet schema registration failed for {DataSetId}", dataSetId);
                return Results.Problem(title: "DataSet schema registration failed");
            }
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("AddDataSetSchema");

        tenantGroup.MapPost("/{dataSetId:guid}/activate", async (Guid tenantId, Guid dataSetId,
            DataSetManagementService service, CancellationToken token) =>
        {
            var result = await service.ActivateAsync(new DataSetId(dataSetId), tenantId, token);
            return result.IsSuccess
                ? Results.NoContent()
                : result.Error.Code == "dataset.not_found"
                    ? Results.NotFound(new { result.Error.Code, result.Error.Description })
                    : Results.Conflict(new { result.Error.Code, result.Error.Description });
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("ActivateDataSet");

        return endpoints;
    }

    public sealed record CreateDataSetRequest(Guid OrganizationId, Guid ProjectId, string Name, string StorageAdapter = "sqlite");
    public sealed record AddDataSetSchemaRequest(string SchemaJson);
}
