using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.Worker.Application.Execution;
using EDAN.Tensor.Worker.Application.Abstractions;

namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>Worker DLQ 운영 기능을 기존 Platform 인증 및 Tenant 관리자 경계로 노출합니다.</summary>
public static class WorkerEndpoints
{
    public static IEndpointRouteBuilder MapWorkerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/worker/dead-letters")
            .WithTags("Worker Operations")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>()
            .AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        group.MapGet("", async (Guid tenantId, WorkerTaskDeadLetterService service,
            CancellationToken token, int page = 1, int pageSize = 50) =>
        {
            try
            {
                var tasks = await service.GetAsync(tenantId, page, pageSize, token);
                return Results.Ok(new
                {
                    Page = page,
                    PageSize = pageSize,
                    Items = tasks.Select(task => new
                    {
                        task.Id,
                        task.TaskType,
                        Status = task.Status.ToString(),
                        task.AttemptCount,
                        task.MaxAttempts,
                        task.LastErrorCode,
                        task.CreatedAtUtc,
                        task.NextAttemptAtUtc
                    })
                });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Code = "worker.dead_letter.invalid_query", Description = ex.Message });
            }
            catch (OverflowException)
            {
                return Results.BadRequest(new { Code = "worker.dead_letter.invalid_page", Description = "페이지 범위가 너무 큽니다." });
            }
        }).WithName("GetWorkerDeadLetters");

        group.MapPost("/{taskId:guid}/replay", async (Guid tenantId, Guid taskId,
            HttpContext context, WorkerTaskDeadLetterService service, CancellationToken token) =>
        {
            var actor = context.User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(actor))
                return Results.Unauthorized();

            var replayRequestHeader = context.Request.Headers["Replay-Request-Id"].ToString();
            if (!Guid.TryParse(replayRequestHeader, out var replayRequestId) || replayRequestId == Guid.Empty)
                return Results.BadRequest(new
                {
                    Code = "worker.dead_letter.replay_request_id_required",
                    Description = "재처리 요청의 멱등성을 위해 유효한 Replay-Request-Id GUID 헤더가 필요합니다."
                });

            try
            {
                var replayTaskId = await service.ReplayAsync(tenantId, taskId, replayRequestId, actor, token);
                return replayTaskId is null
                    ? Results.NotFound(new { Code = "worker.dead_letter.not_found", Description = "Tenant의 DLQ 작업을 찾을 수 없습니다." })
                    : Results.Json(new { OriginalTaskId = taskId, ReplayTaskId = replayTaskId },
                        statusCode: StatusCodes.Status201Created);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Code = "worker.dead_letter.invalid_request", Description = ex.Message });
            }
        }).WithName("ReplayWorkerDeadLetter");

        var taskGroup = endpoints.MapGroup("/api/platform/tenants/{tenantId:guid}/worker/tasks")
            .WithTags("Worker Operations")
            .RequireAuthorization("TenantMember")
            .AddEndpointFilter<TenantRouteAuthorizationFilter>()
            .AddEndpointFilter<TenantMembershipAuthorizationFilter>()
            .AddEndpointFilter<TenantAdministratorAuthorizationFilter>();

        taskGroup.MapGet("/{taskId:guid}", async (Guid tenantId, Guid taskId,
            WorkerTaskQueryService service, CancellationToken token) =>
        {
            var task = await service.GetAsync(tenantId, taskId, token);
            return task is null
                ? Results.NotFound(new { Code = "worker.task.not_found", Description = "Tenant의 작업을 찾을 수 없습니다." })
                : Results.Ok(task);
        }).WithName("GetWorkerTaskStatus");

        taskGroup.MapPost("", async (Guid tenantId, HttpRequest request,
            EnqueueWorkerTaskRequest body, WorkerTaskEnqueueService service, CancellationToken token) =>
        {
            var idempotencyKey = request.Headers["Idempotency-Key"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return Results.BadRequest(new
                {
                    Code = "worker.enqueue.idempotency_key_required",
                    Description = "Idempotency-Key 헤더가 필요합니다."
                });

            try
            {
                var result = await service.EnqueueAsync(tenantId, idempotencyKey,
                    body.TaskType, body.PayloadReference, body.MaxAttempts, token);
                if (result.Conflict)
                    return Results.Conflict(new
                    {
                        Code = "worker.enqueue.idempotency_conflict",
                        Description = "동일 Idempotency Key가 다른 작업 내용으로 이미 사용되었습니다."
                    });

                var response = new
                {
                    result.Task.Id,
                    result.Task.TaskType,
                    Status = result.Task.Status.ToString(),
                    result.Task.CreatedAtUtc
                };
                return result.Created
                    ? Results.Created($"/api/platform/tenants/{tenantId}/worker/tasks/{result.Task.Id}", response)
                    : Results.Ok(response);
            }
            catch (WorkerTaskTypeNotAvailableException ex)
            {
                return Results.UnprocessableEntity(new { Code = "worker.enqueue.task_type_unavailable", Description = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Code = "worker.enqueue.invalid_request", Description = ex.Message });
            }
        }).WithName("EnqueueWorkerTask");

        return endpoints;
    }

    /// <summary>외부 작업 등록에 허용되는 최소 입력입니다. 작업 본문 대신 보안이 적용된 Payload 참조를 전달합니다.</summary>
    public sealed record EnqueueWorkerTaskRequest(string TaskType, string PayloadReference, int MaxAttempts = 5);
}
