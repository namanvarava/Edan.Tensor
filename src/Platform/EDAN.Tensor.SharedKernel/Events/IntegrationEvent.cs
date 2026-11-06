namespace EDAN.Tensor.SharedKernel.Events;

/// <summary>IntegrationEvent는 추적 정보와 Tenant 메타데이터를 포함하는 통합 이벤트 기본 레코드입니다.</summary>
public abstract record IntegrationEvent(Guid EventId, DateTimeOffset OccurredAtUtc, Guid? CorrelationId = null, Guid? CausationId = null, Guid? TenantId = null)
    : DomainEvent(EventId, OccurredAtUtc), IIntegrationEvent
{
    public abstract string EventType { get; }
    public virtual int SchemaVersion => 1;
}
