using EDAN.Tensor.SharedKernel.Events;
namespace EDAN.Tensor.SharedKernel.Primitives;

/// <summary>Aggregate Root는 발행 전까지 도메인 이벤트를 수집합니다.</summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    protected AggregateRoot(TId id) : base(id) { }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void AddDomainEvent(IDomainEvent domainEvent) { ArgumentNullException.ThrowIfNull(domainEvent); _domainEvents.Add(domainEvent); }
    public IReadOnlyCollection<IDomainEvent> DequeueDomainEvents() { IDomainEvent[] events = [.. _domainEvents]; _domainEvents.Clear(); return events; }
}
