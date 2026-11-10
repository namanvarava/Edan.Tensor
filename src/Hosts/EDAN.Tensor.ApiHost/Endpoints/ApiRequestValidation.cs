namespace EDAN.Tensor.ApiHost.Endpoints;

/// <summary>API 요청에서 여러 엔드포인트가 공유하는 입력 제약을 검증합니다.</summary>
internal static class ApiRequestValidation
{
    /// <summary>멱등성 키에 허용하는 최대 문자 수입니다.</summary>
    public const int MaxIdempotencyKeyLength = 200;

    /// <summary>요청 헤더에서 멱등성 키를 읽고 공통 길이 계약을 검증합니다.</summary>
    /// <param name="request">멱등성 키를 읽을 HTTP 요청을 지정합니다.</param>
    /// <param name="requiredCode">키가 없을 때 반환할 오류 코드를 지정합니다.</param>
    /// <param name="tooLongCode">키가 최대 길이를 넘을 때 반환할 오류 코드를 지정합니다.</param>
    /// <param name="value">공백을 제거한 멱등성 키를 반환합니다.</param>
    /// <param name="error">검증에 실패했으면 공개 오류를 반환하고, 성공했으면 null을 반환합니다.</param>
    /// <returns>멱등성 키가 계약에 맞으면 true를 반환합니다.</returns>
    public static bool TryGetIdempotencyKey(
        HttpRequest request,
        string requiredCode,
        string tooLongCode,
        out string value,
        out ApiError? error)
    {
        ArgumentNullException.ThrowIfNull(request);

        value = request.Headers["Idempotency-Key"].ToString().Trim();
        if (value.Length == 0)
        {
            error = new ApiError(requiredCode, "Idempotency-Key 헤더가 필요합니다.");
            return false;
        }

        if (value.Length > MaxIdempotencyKeyLength)
        {
            error = new ApiError(tooLongCode,
                $"Idempotency-Key 헤더는 {MaxIdempotencyKeyLength}자를 초과할 수 없습니다.");
            return false;
        }

        error = null;
        return true;
    }
}
