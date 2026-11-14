using Content.Domain.ProductPages;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;

/// <param name="NameRu">The Russian name, or the Ukrainian one when there is none.</param>
/// <param name="ProductCount">Products on the site in the category and its subcategories, each counted once.</param>
public sealed record PublicCatalogCategory(
    string Slug,
    string NameUk,
    string NameRu,
    int ProductCount,
    IReadOnlyList<PublicCatalogCategory> Children);
