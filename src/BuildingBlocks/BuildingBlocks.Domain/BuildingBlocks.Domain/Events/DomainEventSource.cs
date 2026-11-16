namespace BuildingBlocks.Domain.Events;

/// <summary>
/// The base of an aggregate that raises domain events. The events are not mapped to the database: the members are
/// explicit interface members and the list is a plain field.
/// </summary>
public abstract class DomainEventSource : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    IReadOnlyCollection<IDomainEvent> IHasDomainEvents.DomainEvents => _domainEvents;

    void IHasDomainEvents.ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
