namespace EDAN.Tensor.SharedKernel.Events;

/// <summary>IIntegrationEvent는 모듈 또는 프로세스 경계를 넘는 버전 관리 이벤트 계약입니다.</summary>
public interface IIntegrationEvent : IDomainEvent
{
    string EventType { get; }
    int SchemaVersion { get; }
    Guid? CorrelationId { get; }
    Guid? CausationId { get; }
    Guid? TenantId { get; }
}
