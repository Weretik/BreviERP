namespace BuildingBlocks.Application.Outbox;

/// <summary>The ids of outbox messages that saves in this scope wrote from domain events, kept until they are tried.</summary>
public interface ISavedOutboxMessages
{
    void Add(IEnumerable<Guid> ids);

    /// <summary>The ids saved since the last call.</summary>
    IReadOnlyCollection<Guid> Take();
}
