using BuildingBlocks.Domain.Events;

namespace Catalog.Domain.ProductCategories;

public enum ProductCategoryAction
{
    Created = 1,
    Changed = 2,
    Moved = 3,
    Deleted = 4
}

/// <summary>A staff member created, changed, moved or deleted a category (ACT-001); a change keeps the name and
/// «Активна» before it.</summary>
public sealed record ProductCategoryRecorded(
    ProductCategory Category,
    ProductCategoryAction Action,
    string? NameBefore = null,
    bool? ActiveBefore = null) : IDomainEvent;
