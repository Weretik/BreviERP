using BuildingBlocks.Domain.Events;

namespace Production.Domain.Work;

/// <summary>
/// A person recorded workshop work or voided it (ACT-001); work moved from the old «Цех» is not raised. It carries the
/// record, because a new one gets its id only when it is saved.
/// </summary>
public sealed record ProductionWorkRecorded(ProductionWork Work, bool Voided) : IDomainEvent;
