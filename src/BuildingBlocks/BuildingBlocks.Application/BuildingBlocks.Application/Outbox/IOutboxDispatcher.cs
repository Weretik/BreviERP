namespace BuildingBlocks.Application.Outbox;

/// <summary>Runs saved outbox messages of every module.</summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// The try right after the save that wrote <paramref name="ids"/>. It never throws: a failure stays in the row
    /// for the next dispatch.
    /// </summary>
    Task DispatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>One run of the scheduled dispatch: every due message of every module, a small batch each.</summary>
    Task<OutboxRun> DispatchDueAsync(CancellationToken cancellationToken);
}

public sealed record OutboxRun(int Processed, int Failed)
{
    public static OutboxRun Empty { get; } = new(0, 0);

    public OutboxRun Add(OutboxRun other) => new(Processed + other.Processed, Failed + other.Failed);
}
