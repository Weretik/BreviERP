namespace BuildingBlocks.Infrastructure.Outbox;

/// <summary>Settings of the outbox run.</summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// How long one handler may take (database audit 2026-11-15, 4.2): the claimed rows stay locked in the run's
    /// transaction meanwhile. A handler that runs out of time fails and is tried again later, so a handler must be safe
    /// to run twice (a Telegram message may then be sent twice).
    /// </summary>
    public TimeSpan HandlerTimeout { get; set; } = TimeSpan.FromSeconds(20);
}
