using BuildingBlocks.Domain.Events;

namespace Production.Domain.Orders.Events;

/// <summary>
/// A person changed a «Виробниче завдання» (ACT-001). <paramref name="Lines"/> are the «Історія змін» lines of that
/// action, already display texts; the order is carried because a new one gets its id only when it is saved.
/// </summary>
public sealed record ProductionOrderRecorded(ProductionOrder Order, ProductionOrderActivity Kind,
    IReadOnlyList<ProductionOrderChange> Lines) : IDomainEvent;
