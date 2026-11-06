namespace EDAN.Tensor.SharedKernel.Exceptions;

/// <summary>BusinessRuleViolationException은 작업이 업무 규칙을 위반했음을 나타냅니다.</summary>
public sealed class BusinessRuleViolationException(string code, string message) : DomainException(code, message) { }
