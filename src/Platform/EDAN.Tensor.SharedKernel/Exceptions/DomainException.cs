namespace EDAN.Tensor.SharedKernel.Exceptions;

/// <summary>DomainException은 잘못된 도메인 작업이나 불변식 위반을 나타내는 기본 예외입니다.</summary>
public class DomainException : Exception
{
    public DomainException(string code, string message) : base(message) { ArgumentException.ThrowIfNullOrWhiteSpace(code); Code = code; }
    public string Code { get; }
}
