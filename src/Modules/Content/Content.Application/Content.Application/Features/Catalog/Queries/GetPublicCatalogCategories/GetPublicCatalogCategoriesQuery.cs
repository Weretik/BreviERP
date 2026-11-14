using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;

/// <summary>The category tree of the site with names in both languages (CNT-003 R-02, R-06).</summary>
public sealed record GetPublicCatalogCategoriesQuery : IQuery<Result<PublicCatalogCategoryTree>>;
