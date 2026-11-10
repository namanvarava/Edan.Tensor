using EDAN.Tensor.ApiHost.Endpoints;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace EDAN.Tensor.ApiHost.IntegrationTests;

/// <summary>공통 API 요청 헤더 검증의 경계값과 오류 코드를 확인합니다.</summary>
public sealed class ApiRequestValidationTests
{
    /// <summary>멱등성 키는 공백을 제거한 뒤 200자까지 허용합니다.</summary>
    [Fact]
    public void IdempotencyKeyAcceptsTrimmedValueAtMaximumLength()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = $"  {new string('k', 200)}  ";

        var valid = ApiRequestValidation.TryGetIdempotencyKey(
            context.Request, "required", "too_long", out var value, out var error);

        Assert.True(valid);
        Assert.Equal(new string('k', 200), value);
        Assert.Null(error);
    }

    /// <summary>멱등성 키가 없으면 호출자가 분기할 수 있는 필수 오류를 반환합니다.</summary>
    [Fact]
    public void IdempotencyKeyRejectsMissingValue()
    {
        var context = new DefaultHttpContext();

        var valid = ApiRequestValidation.TryGetIdempotencyKey(
            context.Request, "required", "too_long", out var value, out var error);

        Assert.False(valid);
        Assert.Empty(value);
        Assert.Equal("required", error?.Code);
    }

    /// <summary>멱등성 키가 200자를 넘으면 저장소 작업 전에 길이 오류를 반환합니다.</summary>
    [Fact]
    public void IdempotencyKeyRejectsValueBeyondMaximumLength()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Idempotency-Key"] = new string('k', 201);

        var valid = ApiRequestValidation.TryGetIdempotencyKey(
            context.Request, "required", "too_long", out var value, out var error);

        Assert.False(valid);
        Assert.Equal(new string('k', 201), value);
        Assert.Equal("too_long", error?.Code);
    }
}
