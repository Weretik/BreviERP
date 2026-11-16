using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Events;

namespace Reference.Domain;

/// <summary>
/// The base of a «Довідники» entry: tells «Журнал дій» (ACT-001) that it was created, changed or deleted. Imports and
/// syncs suppress these events in their command.
/// </summary>
public abstract class ReferenceEntity<TId> : BaseEntity<TId>
{
    /// <summary>Tells «Журнал дій» that the entry is being deleted; call it before removing it.</summary>
    public void MarkDeleted() => Raise(new ReferenceItemRecorded(this, ReferenceItemAction.Deleted));

    /// <summary>A new entry: only «created» is listed, not the details its factory applied.</summary>
    protected void RecordCreated()
    {
        ((IHasDomainEvents)this).ClearDomainEvents();
        Raise(new ReferenceItemRecorded(this, ReferenceItemAction.Created));
    }

    protected void RecordChanged(string? nameBefore, decimal? priceBefore = null) =>
        Raise(new ReferenceItemRecorded(this, ReferenceItemAction.Changed, nameBefore, priceBefore));
}
