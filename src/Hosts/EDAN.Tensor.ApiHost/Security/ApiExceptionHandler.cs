using System.Text.Json;
using EDAN.Tensor.ApiHost.Endpoints;
using EDAN.Tensor.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace EDAN.Tensor.ApiHost.Security;

/// <summary>처리되지 않은 API 예외를 공개 오류 계약으로 변환하고 내부 상세정보를 숨깁니다.</summary>
/// <remarks>
/// 예상 가능한 업무 오류는 호출자가 안정적으로 분기할 수 있도록 오류 코드를 보존합니다.
/// 예기치 않은 예외는 상관관계 식별자를 로그에만 기록하고 응답에는 일반 오류 코드만 반환합니다.
/// 요청이 이미 취소된 경우에는 응답을 다시 쓰지 않습니다.
/// </remarks>
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>예외를 상태 코드와 기계 판독용 오류 코드로 변환합니다.</summary>
    /// <param name="httpContext">현재 HTTP 요청과 응답 컨텍스트를 지정합니다.</param>
    /// <param name="exception">처리되지 않은 예외를 지정합니다.</param>
    /// <returns>예외 응답을 기록했으면 true를, 요청이 이미 취소되었으면 false를 반환합니다.</returns>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return false;

        var (statusCode, error) = ToError(exception);
        var traceId = httpContext.TraceIdentifier;
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "처리되지 않은 API 예외를 일반 오류로 변환했습니다. TraceId={TraceId}, Code={Code}",
                traceId, error.Code);
        }
        else
        {
            logger.LogWarning(exception,
                "API 요청 오류를 공개 오류 계약으로 변환했습니다. TraceId={TraceId}, Code={Code}",
                traceId, error.Code);
        }

        httpContext.Response.StatusCode = statusCode;
        await Results.Json(error).ExecuteAsync(httpContext);
        return true;
    }

    private static (int StatusCode, ApiError Error) ToError(Exception exception) => exception switch
    {
        BusinessRuleViolationException rule =>
            (StatusCodes.Status409Conflict, new ApiError(rule.Code, rule.Message)),
        DomainException domain =>
            (StatusCodes.Status400BadRequest, new ApiError(domain.Code, domain.Message)),
        DbUpdateConcurrencyException =>
            (StatusCodes.Status409Conflict,
                new ApiError("api.concurrency_conflict", "동시에 변경된 데이터를 다시 조회한 뒤 재시도해야 합니다.")),
        BadHttpRequestException badRequest when badRequest.InnerException is JsonException =>
            (badRequest.StatusCode,
                new ApiError("api.invalid_json", "요청 본문이 올바른 JSON 형식이 아닙니다.")),
        BadHttpRequestException badRequest =>
            (badRequest.StatusCode,
                new ApiError("api.invalid_request", "요청 형식이 올바르지 않습니다.")),
        JsonException =>
            (StatusCodes.Status400BadRequest,
                new ApiError("api.invalid_json", "요청 본문이 올바른 JSON 형식이 아닙니다.")),
        ArgumentException argument =>
            (StatusCodes.Status400BadRequest, new ApiError("api.invalid_request", argument.Message)),
        InvalidOperationException =>
            (StatusCodes.Status409Conflict,
                new ApiError("api.invalid_operation", "현재 상태에서는 요청을 처리할 수 없습니다.")),
        OperationCanceledException =>
            (StatusCodes.Status503ServiceUnavailable,
                new ApiError("api.operation_cancelled", "요청 처리가 취소되었습니다.")),
        _ =>
            (StatusCodes.Status500InternalServerError,
                new ApiError("api.internal_error", "요청을 처리하는 동안 내부 오류가 발생했습니다."))
    };
}
