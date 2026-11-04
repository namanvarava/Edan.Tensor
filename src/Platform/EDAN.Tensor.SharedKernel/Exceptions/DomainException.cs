namespace EDAN.Tensor.SharedKernel.Exceptions;

/// <summary>Base exception for invalid domain operations or violated invariants.</summary>
public class DomainException : Exception
{
    public DomainException(string code, string message) : base(message) { ArgumentException.ThrowIfNullOrWhiteSpace(code); Code = code; }
    public string Code { get; }
}
