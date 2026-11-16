namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Something that happened to an aggregate, named in the past tense (<c>QuoteCreated</c>). It carries the values that
/// changed; an event of a new row whose id the database generates carries the aggregate itself, because the id is
/// known only after the insert. Raised with <see cref="DomainEventSource.Raise"/>; the module's save turns it into
/// outbox messages in the same transaction (ADR-0016).
/// </summary>
public interface IDomainEvent;
