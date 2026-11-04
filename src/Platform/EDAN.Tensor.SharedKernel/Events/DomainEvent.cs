namespace EDAN.Tensor.SharedKernel.Events;
public abstract record DomainEvent(Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
