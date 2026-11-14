using Catalog.Application.Abstractions.Reference.Items;

namespace Catalog.Application.Abstractions.Reference;

/// <summary>Reference items by sheet code or name. Items without a code are left out.</summary>
public sealed record ProductReferenceCodes(
    IReadOnlyList<ReferenceCodeItem> Fabrics,
    IReadOnlyList<ReferenceCodeItem> Accessories,
    IReadOnlyList<ReferenceCodeItem> Operations,
    IReadOnlyList<ReferenceItem> Suppliers,
    IReadOnlyList<ProductSectionItem> Sections);
