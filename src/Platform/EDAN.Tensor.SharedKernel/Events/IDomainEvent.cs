namespace EDAN.Tensor.SharedKernel.Events;
public interface IDomainEvent { Guid EventId { get; } DateTimeOffset OccurredAtUtc { get; } }
