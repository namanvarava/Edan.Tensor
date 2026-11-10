using System.Diagnostics;

namespace EDAN.Tensor.ApiHost.Observability;

/// <summary>HTTP 요청의 서버 식별자를 응답과 구조화 로그에 연결합니다.</summary>
/// <remarks>
/// 서버가 생성한 <c>X-Request-Id</c>를 성공·오류 응답에 추가하여 호출자가 로그의 요청을 찾을 수 있게 합니다.
/// 요청 본문과 인증 토큰은 기록하지 않으며, 경로·상태 코드·처리 시간만 구조화 로그로 남깁니다.
/// 응답 본문과 기존 공개 오류 계약은 변경하지 않습니다.
/// </remarks>
internal sealed class RequestObservabilityMiddleware(
    RequestDelegate next,
    ILogger<RequestObservabilityMiddleware> logger)
{
    internal const string RequestIdHeaderName = "X-Request-Id";

    /// <summary>현재 HTTP 요청을 실행하고 관측성 정보를 기록합니다.</summary>
    /// <param name="httpContext">현재 HTTP 요청과 응답 컨텍스트를 지정합니다.</param>
    /// <returns>다음 미들웨어가 요청 처리를 끝낼 때 완료되는 작업을 반환합니다.</returns>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var requestId = httpContext.TraceIdentifier;
        httpContext.Response.OnStarting(static state =>
        {
            var context = (HttpContext)state;
            if (!context.Response.Headers.ContainsKey(RequestIdHeaderName))
                context.Response.Headers[RequestIdHeaderName] = context.TraceIdentifier;

            return Task.CompletedTask;
        }, httpContext);

        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            await next(httpContext);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            logger.LogInformation(
                "HTTP 요청 처리를 완료했습니다. Method={Method}, Path={Path}, StatusCode={StatusCode}, RequestId={RequestId}, ElapsedMilliseconds={ElapsedMilliseconds}",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                httpContext.Response.StatusCode,
                requestId,
                elapsed.TotalMilliseconds);
        }
    }
}
