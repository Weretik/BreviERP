using Reference.Domain.ProductSections.Enums;

namespace Reference.Domain.ProductSections.ValueObjects;

/// <summary>Editable fields of a product section: one row of «Типи і розділи».</summary>
public sealed record ProductSectionDetails(
    ProductSectionType Type,
    string Name,
    bool IsActive,
    string? Comment,
    int SortOrder);
