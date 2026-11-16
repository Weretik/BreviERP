namespace BuildingBlocks.Application.Actors;

/// <summary>Who acts in this scope. Null outside a signed-in request (background tasks, seed, public site).</summary>
public interface ICurrentActor
{
    Actor? Current { get; }
}
