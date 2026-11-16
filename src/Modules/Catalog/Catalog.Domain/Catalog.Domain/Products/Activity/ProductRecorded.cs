using BuildingBlocks.Domain.Events;

namespace Catalog.Domain.Products.Activity;

public enum ProductAction
{
    Created = 1,
    Changed = 2,
    Deleted = 3
}

/// <summary>
/// A staff member created, changed or deleted a product (ACT-001). One command raises one event: the parts it touched
/// gather in <see cref="Parts"/>, and the name and «Активний» before it are kept for the field diff.
/// </summary>
public sealed record ProductRecorded(Product Product, ProductAction Action, string NameBefore, bool? ActiveBefore)
    : IDomainEvent
{
    /// <summary>The parts of the card the command changed, in the owner's words, such as «фото» or «склад».</summary>
    public SortedSet<string> Parts { get; } = new(StringComparer.Ordinal);
}
