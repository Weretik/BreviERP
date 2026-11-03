namespace Catalog.Application.Contracts.Reference;

/// <summary>Reference items by sheet code or name. Items without a code are left out.</summary>
public sealed record ProductReferenceCodes(
    IReadOnlyList<ReferenceCodeItem> Fabrics,
    IReadOnlyList<ReferenceCodeItem> Accessories,
    IReadOnlyList<ReferenceCodeItem> Operations,
    IReadOnlyList<ReferenceItem> Suppliers,
    IReadOnlyList<ProductSectionItem> Sections);
