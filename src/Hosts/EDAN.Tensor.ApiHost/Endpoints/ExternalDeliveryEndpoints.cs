using System.Text.Json;
using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.Worker.Application.Abstractions;
using EDAN.Tensor.Worker.Application.ExternalDelivery;
using EDAN.Tensor.Worker.Application.Execution;

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
            var idempotencyKey = httpRequest.Headers["Idempotency-Key"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return Results.BadRequest(new { Code = "external.delivery.idempotency_key_required", Description = "Idempotency-Key 헤더가 필요합니다." });

            var payload = request.Payload.GetRawText();
            var existing = await service.GetByIdempotencyKeyAsync(tenantId, idempotencyKey, token);
            if (existing is not null && (!string.Equals(existing.TargetUri, request.TargetUri, StringComparison.Ordinal)
                || !string.Equals(existing.Payload, payload, StringComparison.Ordinal)))
                return Results.Conflict(new { Code = "external.delivery.idempotency_conflict", Description = "동일 Idempotency-Key가 다른 전달 내용으로 이미 사용되었습니다." });

            Guid? createdDeliveryId = null;
            try
            {
                var delivery = existing ?? await service.CreateAsync(tenantId, request.TargetUri, payload,
                    idempotencyKey, request.MaxAttempts, DateTimeOffset.UtcNow, token);
                if (existing is null)
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
