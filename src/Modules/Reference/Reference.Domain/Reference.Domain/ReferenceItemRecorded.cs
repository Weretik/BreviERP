using BuildingBlocks.Domain.Events;

namespace Reference.Domain;

/// <summary>What happened to a «Довідники» entry, for «Журнал дій» (ACT-001).</summary>
public enum ReferenceItemAction
{
    Created,
    Changed,
    Deleted,
}

/// <summary>
/// A person created, changed or deleted a «Довідники» entry (ACT-001). <paramref name="NameBefore"/> and
/// <paramref name="PriceBefore"/> are the values before a change; the entry itself carries the new ones.
/// </summary>
public sealed record ReferenceItemRecorded(object Item, ReferenceItemAction Action, string? NameBefore = null,
    decimal? PriceBefore = null) : IDomainEvent;
