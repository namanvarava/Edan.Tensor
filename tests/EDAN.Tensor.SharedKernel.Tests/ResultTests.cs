using EDAN.Tensor.SharedKernel.Results;
using Xunit;
namespace EDAN.Tensor.SharedKernel.Tests;
public sealed class ResultTests
{
    [Fact] public void SuccessResult_ExposesValue() { var result = Result<int>.Success(42); Assert.True(result.IsSuccess); Assert.Equal(42, result.Value); Assert.True(result.Error.IsNone); }
    [Fact] public void FailureResult_ExposesErrorAndRejectsValueAccess() { var error = new Error("ORDER_INVALID", "The order is invalid."); var result = Result<int>.Failure(error); Assert.True(result.IsFailure); Assert.Equal(error, result.Error); Assert.Throws<InvalidOperationException>(() => result.Value); }
    [Fact] public void NonGenericResult_RepresentsSuccessAndFailure() { Assert.True(Result.Success().IsSuccess); Assert.Equal("DENIED", Result.Failure(new Error("DENIED", "Denied.")).Error.Code); }
}
