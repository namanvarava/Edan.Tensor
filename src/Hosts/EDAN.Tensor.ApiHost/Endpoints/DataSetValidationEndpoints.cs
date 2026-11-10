using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.DataSet.Application.UseCases;
using EDAN.Tensor.DataSet.Application.Validation;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.Execution;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>DataSet 검증 프로필·실행·결과 조회를 Tenant 권한 경계로 노출합니다.</summary>
public static class DataSetValidationEndpoints
{
    /// <summary>승인된 v1 검증 계약의 HTTP 엔드포인트를 등록합니다.</summary>
    public static IEndpointRouteBuilder MapDataSetValidationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var memberGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/datasets")
            .WithTags("DataSet Validation")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        memberGroup.MapGet("/validations/{executionId:guid}", async (
            Guid tenantId, Guid executionId, DataSetValidationService service, CancellationToken token) =>
        {
            var execution = await service.GetExecutionAsync(tenantId, executionId, token);
            return execution is null
                ? Results.NotFound(new { Code = "dataset.validation.not_found", Description = "Tenant 범위에서 검증 실행을 찾을 수 없습니다." })
                : Results.Ok(ToResponse(execution));
        }).WithName("GetDataSetValidation");

        memberGroup.MapGet("/validations/{executionId:guid}/details", async (
            Guid tenantId, Guid executionId, int? skip, int? take,
            DataSetValidationService service, CancellationToken token) =>
        {
            if (await service.GetExecutionAsync(tenantId, executionId, token) is null)
                return Results.NotFound(new { Code = "dataset.validation.not_found", Description = "Tenant 범위에서 검증 실행을 찾을 수 없습니다." });
            try
            {
                var items = await service.GetDetailsAsync(tenantId, executionId, skip ?? 0, take ?? 100, token);
                return Results.Ok(new { Skip = skip ?? 0, Take = take ?? 100, Items = items });
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Results.BadRequest(new { Code = "dataset.validation.invalid_query", Description = exception.Message });
            }
        }).WithName("GetDataSetValidationDetails");

        var adminGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/datasets")
            .WithTags("DataSet Validation")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>()
            .AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        adminGroup.MapPost("/validation-profiles", async (
            Guid tenantId, CreateDataSetValidationProfileRequest request,
            DataSetValidationService service, CancellationToken token) =>
        {
            var result = await service.CreateProfileAsync(tenantId, request.Name, request.DefinitionJson,
                DateTimeOffset.UtcNow, token);
            return result.IsSuccess
                ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created)
                : Results.BadRequest(new { result.Error.Code, result.Error.Description });
        }).WithName("CreateDataSetValidationProfile");

        adminGroup.MapPost("/validation-profiles/{profileId:guid}/revisions", async (
            Guid tenantId, Guid profileId, CreateDataSetValidationRevisionRequest request,
            DataSetValidationService service, CancellationToken token) =>
        {
            var result = await service.AddProfileRevisionAsync(tenantId, profileId, request.DefinitionJson,
                DateTimeOffset.UtcNow, token);
            return result.IsSuccess
                ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created)
                : result.Error.Code == "dataset.validation.profile_not_found"
                    ? Results.NotFound(new { result.Error.Code, result.Error.Description })
                    : Results.BadRequest(new { result.Error.Code, result.Error.Description });
        }).WithName("CreateDataSetValidationProfileRevision");

        adminGroup.MapPost("/{dataSetId:guid}/validations", async (
            Guid tenantId, Guid dataSetId, HttpRequest httpRequest,
            CreateDataSetValidationRequest request, DataSetValidationService service,
            WorkerTaskEnqueueService enqueue, CancellationToken token) =>
        {
            var idempotencyKey = httpRequest.Headers["Idempotency-Key"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return Results.BadRequest(new
                {
                    Code = "dataset.validation.idempotency_key_required",
                    Description = "검증 실행의 중복 예약을 막으려면 Idempotency-Key 헤더가 필요합니다."
                });

            var result = await service.CreateExecutionWithStatusAsync(tenantId, dataSetId,
                new DataSetValidationRequest(request.ExpectedDataSetVersion, request.SchemaVersion,
                    request.ProfileId, request.ProfileRevision), idempotencyKey, DateTimeOffset.UtcNow, token);
            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    "dataset.not_found" => Results.NotFound(new { result.Error.Code, result.Error.Description }),
                    "dataset.validation.idempotency_conflict" => Results.Conflict(new { result.Error.Code, result.Error.Description }),
                    _ => Results.BadRequest(new { result.Error.Code, result.Error.Description })
                };
            }

            var createdExecutionId = result.Value.Created ? result.Value.Execution.Id : (Guid?)null;
            try
            {
                var task = await enqueue.EnqueueAsync(tenantId, idempotencyKey,
                    DataSetValidationTaskContract.TaskType,
                    DataSetValidationTaskContract.CreatePayloadReference(result.Value.Execution.Id), 5, token);
                if (task.Conflict)
                {
                    if (createdExecutionId.HasValue)
                        await service.DeleteUnqueuedExecutionAsync(tenantId, createdExecutionId.Value, CancellationToken.None);
                    return Results.Conflict(new { Code = "dataset.validation.idempotency_conflict", Description = "동일 Idempotency-Key가 다른 작업으로 이미 사용되었습니다." });
                }
                return Results.Accepted($"/api/platform/tenants/{tenantId}/datasets/validations/{result.Value.Execution.Id}",
                    new { Execution = ToResponse(result.Value.Execution), WorkerTaskId = task.Task.Id, task.Created });
            }
            catch (WorkerTaskTypeNotAvailableException)
            {
                if (createdExecutionId.HasValue)
                    await service.DeleteUnqueuedExecutionAsync(tenantId, createdExecutionId.Value, CancellationToken.None);
                return Results.Json(new ApiError("worker.task_type_unavailable",
                    "검증 작업을 실행할 수 있는 Worker 작업 유형이 등록되지 않았습니다."),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }).WithName("CreateDataSetValidation");

        adminGroup.MapDelete("/validations/{executionId:guid}", async (
            Guid tenantId, Guid executionId, DataSetValidationService service, CancellationToken token) =>
        {
            try
            {
                var deleted = await service.DeleteExecutionAsync(tenantId, executionId, token);
                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { Code = "dataset.validation.not_found", Description = "Tenant 범위에서 검증 실행을 찾을 수 없습니다." });
            }
            catch (InvalidOperationException exception)
            {
                return Results.Conflict(new { Code = "dataset.validation.in_progress", Description = exception.Message });
            }
        }).WithName("DeleteDataSetValidation");

        return endpoints;
    }

    private static object ToResponse(DataSetValidationExecutionSnapshot execution) => new
    {
        execution.Id,
        execution.TenantId,
        execution.DataSetId,
        execution.ExpectedDataSetVersion,
        execution.SchemaVersion,
        execution.ProfileId,
        execution.ProfileRevision,
        Status = execution.Status.ToString(),
        execution.DetailReference,
        execution.ProcessedRecords,
        execution.TotalRecords,
        execution.ErrorCount,
        execution.WarningCount,
        execution.DetailItemCount,
        execution.DetailTruncated,
        execution.RuleCountsJson,
        execution.FailureCode,
        execution.FailureMessage,
        execution.CreatedAtUtc,
        execution.CompletedAtUtc
    };

    /// <summary>Tenant 관리자가 생성할 검증 프로필 입력입니다.</summary>
    public sealed record CreateDataSetValidationProfileRequest(string Name, string DefinitionJson);

    /// <summary>Tenant 관리자가 추가할 검증 프로필 revision 입력입니다.</summary>
    public sealed record CreateDataSetValidationRevisionRequest(string DefinitionJson);

    /// <summary>고정된 DataSet·스키마·프로필 revision으로 검증 실행을 예약하는 입력입니다.</summary>
    public sealed record CreateDataSetValidationRequest(
        int ExpectedDataSetVersion, int SchemaVersion, Guid ProfileId, int ProfileRevision);
}
