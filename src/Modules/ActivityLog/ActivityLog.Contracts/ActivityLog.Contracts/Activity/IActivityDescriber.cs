namespace ActivityLog.Contracts.Activity;

/// <summary>
/// How a module's domain events read in «Журнал дій» (ADR-0016). A module registers one describer for its events;
/// ActivityLog adds who did it and when and writes the message into the module's outbox in the same transaction.
/// </summary>
public interface IActivityDescriber
{
    /// <returns>The description, or null when the event is not this module's or is not a person's action.</returns>
    ActivityDescription? Describe(object domainEvent);
}
