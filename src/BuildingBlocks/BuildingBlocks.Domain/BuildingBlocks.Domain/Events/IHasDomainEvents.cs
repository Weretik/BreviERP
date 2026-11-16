namespace BuildingBlocks.Domain.Events;

/// <summary>An entity that keeps the events it raised until its module saves it.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
