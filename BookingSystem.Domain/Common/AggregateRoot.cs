namespace BookingSystem.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public AggregateRoot<TId> WithDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        return this;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}