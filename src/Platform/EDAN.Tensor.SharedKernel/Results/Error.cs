namespace EDAN.Tensor.SharedKernel.Results;

/// <summary>Error는 안정적으로 식별할 수 있는 기계 판독용 오류 정보입니다.</summary>
public sealed record Error(string Code, string Description)
{
    public static Error None { get; } = new(string.Empty, string.Empty);
    public bool IsNone => string.IsNullOrWhiteSpace(Code) && string.IsNullOrWhiteSpace(Description);
}
