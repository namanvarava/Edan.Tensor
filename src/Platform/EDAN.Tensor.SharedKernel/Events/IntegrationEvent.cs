namespace EDAN.Tensor.SharedKernel.Events;

/// <summary>Base record for integration events with tracing and tenant metadata.</summary>
public abstract record IntegrationEvent(Guid EventId, DateTimeOffset OccurredAtUtc, Guid? CorrelationId = null, Guid? CausationId = null, Guid? TenantId = null)
    : DomainEvent(EventId, OccurredAtUtc), IIntegrationEvent
{
    public abstract string EventType { get; }
    public virtual int SchemaVersion => 1;
}
