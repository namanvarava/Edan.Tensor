using System.Text.Json;
using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.ExternalDelivery;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Domain.Model;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>외부 전달 생성과 상태 조회를 Tenant 관리자 경계로 노출합니다.</summary>
public static class ExternalDeliveryEndpoints
{
    /// <summary>외부 전달 API를 등록합니다.</summary>
    public static IEndpointRouteBuilder MapExternalDeliveryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var memberGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/worker/deliveries")
            .WithTags("External Delivery")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>();

        memberGroup.MapGet("", async (Guid tenantId, ExternalDeliveryStatus? status,
            string? targetHost, string? errorCode, DateTimeOffset? fromUtc, DateTimeOffset? toUtc,
            ExternalDeliveryService service, CancellationToken token, int page = 1, int pageSize = 50) =>
        {
            try
            {
                var result = await service.GetPageAsync(tenantId, status, targetHost, errorCode,
                    fromUtc, toUtc, page, pageSize, token);
                return Results.Ok(new
                {
                    result.TotalCount,
                    result.Page,
                    result.PageSize,
                    Items = result.Items.Select(ToResponse)
                });
            }
            catch (ArgumentOutOfRangeException ex) { return Results.BadRequest(new ApiError("external.delivery.invalid_query", ex.Message)); }
            catch (OverflowException) { return Results.BadRequest(new ApiError("external.delivery.invalid_page", "페이지 범위가 너무 큽니다.")); }
        }).WithName("GetExternalDeliveries");

        memberGroup.MapGet("/{deliveryId:guid}", async (Guid tenantId, Guid deliveryId,
            ExternalDeliveryService service, CancellationToken token) =>
        {
            var delivery = await service.GetAsync(tenantId, deliveryId, token);
            return delivery is null
                ? Results.NotFound(new { Code = "external.delivery.not_found", Description = "Tenant 범위에서 외부 전달을 찾을 수 없습니다." })
                : Results.Ok(ToResponse(delivery));
        }).WithName("GetExternalDelivery");

        var adminGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/worker/deliveries")
            .WithTags("External Delivery")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>()
            .AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        adminGroup.MapPost("", async (Guid tenantId, HttpRequest httpRequest,
            CreateExternalDeliveryRequest request, ExternalDeliveryService service,
            WorkerTaskEnqueueService enqueue, CancellationToken token) =>
        {
            if (!ApiRequestValidation.TryGetIdempotencyKey(httpRequest,
                    "external.delivery.idempotency_key_required",
                    "external.delivery.idempotency_key_too_long",
                    out var idempotencyKey, out var headerError))
                return Results.BadRequest(headerError);

            Guid? createdDeliveryId = null;
            try
            {
                var payload = request.Payload.GetRawText();
                var createResult = await service.CreateOrGetAsync(tenantId, request.TargetUri, payload,
                    idempotencyKey, request.MaxAttempts, DateTimeOffset.UtcNow, token);
                var delivery = createResult.Delivery;
                if (!string.Equals(delivery.TargetUri, request.TargetUri, StringComparison.Ordinal)
                    || !string.Equals(delivery.Payload, payload, StringComparison.Ordinal)
                    || delivery.MaxAttempts != request.MaxAttempts)
                    return Results.Conflict(new { Code = "external.delivery.idempotency_conflict", Description = "동일 Idempotency-Key가 다른 전달 내용으로 이미 사용되었습니다." });
                if (createResult.Created)
                    createdDeliveryId = delivery.Id;
                var task = await enqueue.EnqueueAsync(tenantId, idempotencyKey,
                    ExternalDeliveryTaskContract.TaskType,
                    ExternalDeliveryTaskContract.CreatePayloadReference(delivery.Id),
                    request.MaxAttempts, token);
                if (task.Conflict)
                {
                    if (createdDeliveryId.HasValue)
                        await service.DeleteUnqueuedAsync(tenantId, createdDeliveryId.Value, idempotencyKey,
                            CancellationToken.None);
                    return Results.Conflict(new { Code = "external.delivery.idempotency_conflict", Description = "동일 Idempotency-Key가 다른 Worker 작업으로 이미 사용되었습니다." });
                }
                return Results.Accepted($"/api/platform/tenants/{tenantId}/worker/deliveries/{delivery.Id}",
                    new { Delivery = ToResponse(delivery), WorkerTaskId = task.Task.Id, task.Created });
            }
            catch (WorkerTaskTypeNotAvailableException)
            {
                if (createdDeliveryId.HasValue)
                    await service.DeleteUnqueuedAsync(tenantId, createdDeliveryId.Value, idempotencyKey,
                        CancellationToken.None);
                return Results.Json(new ApiError("worker.task_type_unavailable",
                    "외부 전달 작업을 실행할 수 있는 Worker 작업 유형이 등록되지 않았습니다."),
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { Code = "external.delivery.invalid_request", Description = exception.Message });
            }
        }).WithName("CreateExternalDelivery");

        adminGroup.MapPost("/{deliveryId:guid}/retry", async (Guid tenantId, Guid deliveryId,
            HttpRequest httpRequest, ExternalDeliveryService service, WorkerTaskEnqueueService enqueue,
            CancellationToken token) =>
        {
            if (!ApiRequestValidation.TryGetIdempotencyKey(httpRequest,
                    "external.delivery.retry_idempotency_key_required",
                    "external.delivery.retry_idempotency_key_too_long",
                    out var idempotencyKey, out var headerError))
                return Results.BadRequest(headerError);
            try
            {
                var delivery = await service.PrepareRetryAsync(tenantId, deliveryId, token);
                if (delivery is null)
                    return Results.NotFound(new ApiError("external.delivery.not_found", "Tenant 범위에서 외부 전달을 찾을 수 없습니다."));
                var task = await enqueue.EnqueueAsync(tenantId, idempotencyKey,
                    ExternalDeliveryTaskContract.TaskType,
                    ExternalDeliveryTaskContract.CreatePayloadReference(delivery.Id),
                    delivery.MaxAttempts, token);
                if (task.Conflict)
                    return Results.Conflict(new ApiError("external.delivery.retry_idempotency_conflict",
                        "동일 Idempotency-Key가 다른 Worker 작업으로 이미 사용되었습니다."));
                return Results.Accepted($"/api/platform/tenants/{tenantId}/worker/deliveries/{delivery.Id}",
                    new { Delivery = ToResponse(delivery), WorkerTaskId = task.Task.Id, task.Created });
            }
            catch (WorkerTaskTypeNotAvailableException) { return Results.StatusCode(StatusCodes.Status503ServiceUnavailable); }
            catch (InvalidOperationException ex) { return Results.Conflict(new ApiError("external.delivery.retry_not_allowed", ex.Message)); }
        }).WithName("RetryExternalDelivery");

        return endpoints;
    }

    private static object ToResponse(ExternalDeliverySnapshot delivery) => new
    {
        delivery.Id,
        delivery.TenantId,
        delivery.TargetUri,
        delivery.IdempotencyKey,
        Status = delivery.Status.ToString(),
        delivery.AttemptCount,
        delivery.MaxAttempts,
        delivery.LastErrorCode,
        delivery.CreatedAtUtc,
        delivery.CompletedAtUtc
    };

    /// <summary>외부 전달 대상과 JSON 본문을 지정합니다.</summary>
    public sealed record CreateExternalDeliveryRequest(string TargetUri, JsonElement Payload, int MaxAttempts = 5);
}
