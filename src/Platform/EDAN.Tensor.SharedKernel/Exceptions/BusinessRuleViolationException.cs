namespace EDAN.Tensor.SharedKernel.Exceptions;

/// <summary>Indicates that an operation violates a business rule.</summary>
public sealed class BusinessRuleViolationException(string code, string message) : DomainException(code, message) { }
