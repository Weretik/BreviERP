using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;

/// <summary>
/// The category tree of the site (CNT-003 R-06): shown categories in their order, each with its shown subcategories.
/// </summary>
public sealed record PublicCatalogCategoryTree(IReadOnlyList<PublicCatalogCategory> Categories);
