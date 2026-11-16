namespace BuildingBlocks.Application.DomainEvents;

/// <summary>
/// Lets an import or sync say that the rows it saves are not separate actions (ACT-001 R-08): after
/// <see cref="Suppress"/>, the saves of this request drop the domain events of their entities instead of writing them.
/// </summary>
public interface IDomainEventSuppression
{
    bool IsSuppressed { get; }

    void Suppress();
}
