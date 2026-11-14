namespace BuildingBlocks.Application.Outbox;

/// <summary>
/// Runs the outbox messages of one <see cref="Type"/>. A message may come twice (a stop between the work and the
/// mark), so the handler checks its own record first and does nothing when the work is done.
/// </summary>
public interface IOutboxHandler
{
    string Type { get; }

    /// <returns>Null when the work is done; otherwise why it should be tried again later. An exception counts as
    /// «try later» too.</returns>
    Task<string?> HandleAsync(string payload, CancellationToken cancellationToken);
}
