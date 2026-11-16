using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Application.DomainEvents;

/// <summary>
/// A module's translation of its own domain events into outbox messages, called by the save that carries the events
/// (ADR-0016). Returns nothing for events it does not know, so every translation sees every event of the save.
/// </summary>
public interface IDomainEventOutbox
{
    IReadOnlyList<OutboxMessage> Write(IDomainEvent domainEvent, Actor? actor, DateTimeOffset now);
}
