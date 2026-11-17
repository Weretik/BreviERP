namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Tells people that an outbox message failed <see cref="OutboxMessage.MaxAttempts"/> times and will not be tried again
/// (EVT-07). Called once per message, after the failure is saved; a notifier that throws is logged and ignored.
/// </summary>
public interface IOutboxFailureNotifier
{
    Task NotifyGaveUpAsync(OutboxFailure failure, CancellationToken cancellationToken);
}

/// <summary>An outbox message that stopped being retried, with its last error.</summary>
public sealed record OutboxFailure(Guid Id, string Type, int Attempts, string? LastError, DateTimeOffset CreatedAt);
