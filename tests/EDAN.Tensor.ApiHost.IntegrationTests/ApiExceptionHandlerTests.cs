using System.Text.Json;
using EDAN.Tensor.ApiHost.Security;
using EDAN.Tensor.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>API 공통 예외 처리기가 안정적인 오류 계약과 내부 정보 비노출을 지키는지 검증합니다.</summary>
public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task BusinessRuleViolationPreservesCodeAndReturnsConflict()
    {
        var response = await HandleAsync(new BusinessRuleViolationException(
            "test.conflict", "업무 규칙 충돌입니다."));

        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        Assert.Equal("test.conflict", response.Error!.Value.Code);
        Assert.Equal("업무 규칙 충돌입니다.", response.Error.Value.Description);
    }

    [Fact]
    public async Task InvalidOperationReturnsStableConflictWithoutExceptionMessage()
    {
        var response = await HandleAsync(new InvalidOperationException("비밀 내부 상태"));

        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        Assert.Equal("api.invalid_operation", response.Error!.Value.Code);
        Assert.DoesNotContain("비밀 내부 상태", response.Error.Value.Description);
    }

    [Fact]
    public async Task UnknownExceptionReturnsGenericInternalError()
    {
        var response = await HandleAsync(new Exception("비밀 내부 예외"));

        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        Assert.Equal("api.internal_error", response.Error!.Value.Code);
        Assert.DoesNotContain("비밀 내부 예외", response.Error.Value.Description);
    }

    private static async Task<(int StatusCode, (string Code, string Description)? Error)> HandleAsync(Exception exception)
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "api-error-test",
            RequestServices = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .BuildServiceProvider()
        };
        context.Response.Body = new MemoryStream();
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        Assert.True(await handler.TryHandleAsync(context, exception, CancellationToken.None));

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        var root = document.RootElement;
        return (context.Response.StatusCode,
            (root.GetProperty("code").GetString()!, root.GetProperty("description").GetString()!));
    }
}
