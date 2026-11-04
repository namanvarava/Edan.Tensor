namespace EDAN.Tensor.SharedKernel.Events;

/// <summary>Versioned event contract crossing bounded-context or process boundaries.</summary>
public interface IIntegrationEvent : IDomainEvent
{
    string EventType { get; }
    int SchemaVersion { get; }
    Guid? CorrelationId { get; }
    Guid? CausationId { get; }
    Guid? TenantId { get; }
}
