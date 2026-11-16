using BuildingBlocks.Domain.Events;

namespace Warehouse.Domain.Stock.Movements;

/// <summary>
/// A person moved fabric: opening, receipt, issue, adjustment or «Сторно» (ACT-001); movements moved from the old «Цех»
/// are not raised. It carries the movement, whose id is known only after the save.
/// </summary>
public sealed record FabricMovementRecorded(FabricMovement Movement) : IDomainEvent;

/// <summary>A person edited, voided or restored a movement (ACT-001).</summary>
public sealed record FabricMovementRevised(FabricMovementRevision Revision) : IDomainEvent;
