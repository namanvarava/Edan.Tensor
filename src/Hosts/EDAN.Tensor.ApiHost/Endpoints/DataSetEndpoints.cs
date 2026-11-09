using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.ApiHost.Services;
using EDAN.Tensor.HostServices.Services;
using EDAN.Tensor.DataSet.Application.Abstractions;
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
                var result = await service.CreateAsync(scope, request.Name, request.StorageAdapter, request.ContentFormat, token);
                if (!result.IsSuccess)
                    return Results.BadRequest(new { result.Error.Code, result.Error.Description });

                var id = result.Value.Value;
                return Results.Created($"/api/platform/tenants/{tenantId}/datasets/{id}",
                    new { Id = id, TenantId = tenantId, request.OrganizationId, request.ProjectId, request.Name, request.StorageAdapter, request.ContentFormat, Status = "Draft" });
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
                    dataSet.ContentFormat,
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

        tenantGroup.MapPost("/{dataSetId:guid}/content", async (Guid tenantId, Guid dataSetId,
            HttpRequest request, DataSetManagementService service, IConfiguration configuration, CancellationToken token) =>
        {
            var maxUploadBytes = configuration.GetValue<long>("DataSetContent:MaxUploadBytes", 67_108_864);
            if (request.ContentLength is long contentLength && contentLength > maxUploadBytes)
                return Results.Json(new ApiError("dataset.content_too_large",
                    "요청 콘텐츠가 허용된 최대 크기를 초과했습니다."),
                    statusCode: StatusCodes.Status413PayloadTooLarge);

            var result = await service.WriteContentAsync(new DataSetId(dataSetId), tenantId, request.Body, token);
            if (result.IsSuccess)
                return Results.NoContent();
            return result.Error.Code switch
            {
                "dataset.not_found" => Results.NotFound(new { result.Error.Code, result.Error.Description }),
                "dataset.content_too_large" => Results.Json(new ApiError(result.Error.Code, result.Error.Description),
                    statusCode: StatusCodes.Status413PayloadTooLarge),
                "dataset.archived" or "dataset.quota_exceeded" => Results.Conflict(new { result.Error.Code, result.Error.Description }),
                _ => Results.BadRequest(new { result.Error.Code, result.Error.Description })
            };
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("WriteDataSetContent");

        tenantGroup.MapGet("/{dataSetId:guid}/content", async (Guid tenantId, Guid dataSetId,
            DataSetManagementService service, CancellationToken token) =>
        {
            var content = await service.OpenContentReadAsync(new DataSetId(dataSetId), tenantId, token);
            return content is null
                ? Results.NotFound(new { Code = "dataset.content_not_found", Description = "Tenant 범위에서 콘텐츠를 찾을 수 없습니다." })
                : Results.Stream(content, "application/octet-stream");
        }).WithName("ReadDataSetContent");

        tenantGroup.MapGet("/{dataSetId:guid}/usage", async (Guid tenantId, Guid dataSetId,
            DataSetManagementService service, EDAN.Tensor.DataSet.Application.Abstractions.IDataSetStorageUsageReader usageReader,
            EDAN.Tensor.DataSet.Application.Abstractions.IDataSetStorageQuotaPolicyProvider quotaProvider,
            CancellationToken token) =>
        {
            var dataSet = await service.GetAsync(new DataSetId(dataSetId), tenantId, token);
            if (dataSet is null)
                return Results.NotFound(new { Code = "dataset.not_found", Description = "Tenant 범위에서 DataSet을 찾을 수 없습니다." });
            var allocation = new DataSetStorageAllocation(dataSet.Id, dataSet.Scope);
            var usage = await usageReader.GetUsageAsync(allocation, token);
            var policy = await quotaProvider.GetPolicyAsync(allocation, token);
            double? Percent(long bytes, long? limit) => limit is > 0 ? Math.Round(bytes * 100d / limit.Value, 2) : null;
            return Results.Ok(new
                {
                    TenantBytes = usage.TenantBytes,
                    TenantDataSetCount = usage.TenantDataSetCount,
                    OrganizationBytes = usage.OrganizationBytes,
                    OrganizationDataSetCount = usage.OrganizationDataSetCount,
                    ProjectBytes = usage.ProjectBytes,
                    ProjectDataSetCount = usage.ProjectDataSetCount,
                    usage.DataSetBytes,
                    Limits = new { policy.TenantLimitBytes, policy.OrganizationLimitBytes, policy.ProjectLimitBytes, policy.DataSetLimitBytes },
                    UtilizationPercent = new { Tenant = Percent(usage.TenantBytes, policy.TenantLimitBytes), Organization = Percent(usage.OrganizationBytes, policy.OrganizationLimitBytes), Project = Percent(usage.ProjectBytes, policy.ProjectLimitBytes), DataSet = Percent(usage.DataSetBytes, policy.DataSetLimitBytes) },
                    WarningThresholdPercent = policy.WarningThresholdPercent,
                    Warnings = new[]
                    {
                        new { Scope = "Tenant", Active = Percent(usage.TenantBytes, policy.TenantLimitBytes) is double tenantPercent && tenantPercent >= policy.WarningThresholdPercent },
                        new { Scope = "Organization", Active = Percent(usage.OrganizationBytes, policy.OrganizationLimitBytes) is double organizationPercent && organizationPercent >= policy.WarningThresholdPercent },
                        new { Scope = "Project", Active = Percent(usage.ProjectBytes, policy.ProjectLimitBytes) is double projectPercent && projectPercent >= policy.WarningThresholdPercent },
                        new { Scope = "DataSet", Active = Percent(usage.DataSetBytes, policy.DataSetLimitBytes) is double dataSetPercent && dataSetPercent >= policy.WarningThresholdPercent }
                    }.Where(x => x.Active).Select(x => x.Scope).ToArray(),
                    MeasuredAtUtc = DateTimeOffset.UtcNow
                });
        }).WithName("GetDataSetStorageUsage");
        tenantGroup.MapPost("/{dataSetId:guid}/schemas", async (Guid tenantId, Guid dataSetId,
            AddDataSetSchemaRequest request, DataSetManagementService service, CancellationToken token) =>
        {
            var result = await service.AddSchemaAsync(new DataSetId(dataSetId), tenantId,
                request.SchemaJson, DateTimeOffset.UtcNow, token);
            return result.IsSuccess
                ? Results.Created($"/api/platform/tenants/{tenantId}/datasets/{dataSetId}",
                    new { DataSetId = dataSetId, SchemaVersion = result.Value })
                : result.Error.Code == "dataset.not_found"
                    ? Results.NotFound(new ApiError(result.Error.Code, result.Error.Description))
                    : Results.BadRequest(new ApiError(result.Error.Code, result.Error.Description));
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

        var tenantUsageGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}")
            .WithTags("DataSet")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        tenantUsageGroup.MapGet("/storage-usage", async (Guid tenantId,
            IDataSetTenantStorageUsageReader usageReader, CancellationToken token) =>
        {
            var usage = await usageReader.GetTenantUsageAsync(tenantId, token);
            return Results.Ok(new
            {
                usage.TenantBytes,
                usage.TenantDataSetCount,
                MeasuredAtUtc = DateTimeOffset.UtcNow
            });
        }).WithName("GetTenantStorageUsage");

        tenantUsageGroup.MapGet("/storage-usage/history", async (Guid tenantId, int? take,
            DataSetStorageUsageSnapshotService snapshotService, CancellationToken token) =>
        {
            var history = await snapshotService.GetHistoryAsync(tenantId, take ?? 168, token);
            return Results.Ok(history.Select(x => new
            {
                x.PeriodStartUtc, x.CapturedAtUtc, x.UsedBytes, x.DataSetCount
            }));
        }).WithName("GetTenantStorageUsageHistory");

        tenantUsageGroup.MapGet("/storage-usage/quota-events", async (Guid tenantId, int? take,
            DataSetStorageUsageSnapshotService snapshotService, CancellationToken token) =>
        {
            var history = await snapshotService.GetQuotaEventHistoryAsync(tenantId, take ?? 100, token);
            return Results.Ok(history.Select(x => new
            {
                x.PolicyId, x.Scope, x.TargetId, x.EventType, x.UsedBytes, x.LimitBytes,
                x.UtilizationPercent, x.PolicyVersion, x.OccurredAtUtc
            }));
        }).AddEndpointFilter<TenantAdministratorAuthorizationFilter>().WithName("GetTenantQuotaEventHistory");

        return endpoints;
    }

    public sealed record CreateDataSetRequest(Guid OrganizationId, Guid ProjectId, string Name, string StorageAdapter = "sqlite", string ContentFormat = "json-array");
    public sealed record AddDataSetSchemaRequest(string SchemaJson);
}
