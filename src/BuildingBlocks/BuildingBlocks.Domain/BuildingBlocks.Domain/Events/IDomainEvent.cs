namespace BuildingBlocks.Domain.Events;

/// <summary>
/// Something that happened to an aggregate, named in the past tense (<c>QuoteCreated</c>). It carries ids and the
/// values that changed, not the aggregate itself. Raised with <see cref="DomainEventSource.Raise"/>; the module's save
/// turns it into outbox messages in the same transaction (ADR-0016).
/// </summary>
public interface IDomainEvent;
