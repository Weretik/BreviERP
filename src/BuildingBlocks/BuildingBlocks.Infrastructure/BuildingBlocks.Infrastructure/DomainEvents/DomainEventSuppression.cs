using BuildingBlocks.Application.DomainEvents;

namespace BuildingBlocks.Infrastructure.DomainEvents;

public sealed class DomainEventSuppression : IDomainEventSuppression
{
    public bool IsSuppressed { get; private set; }

    public void Suppress() => IsSuppressed = true;
}
