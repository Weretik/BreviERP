using BuildingBlocks.Application.Actors;

namespace BuildingBlocks.Infrastructure.DomainEvents;

/// <summary>The actor of hosts without a signed-in person (Host.Seed): nobody, so nothing is recorded as a person's action.</summary>
internal sealed class NoCurrentActor : ICurrentActor
{
    public Actor? Current => null;
}
