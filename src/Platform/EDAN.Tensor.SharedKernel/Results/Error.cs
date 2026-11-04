namespace EDAN.Tensor.SharedKernel.Results;

/// <summary>Stable machine-readable error information.</summary>
public sealed record Error(string Code, string Description)
{
    public static Error None { get; } = new(string.Empty, string.Empty);
    public bool IsNone => string.IsNullOrWhiteSpace(Code) && string.IsNullOrWhiteSpace(Description);
}
