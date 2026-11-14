using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;

/// <summary>A category page of the site by its address: SEO, text, parent and subcategories. Hidden is «not found».</summary>
public sealed record GetPublicCatalogCategoryQuery(string Slug, string Language)
    : IQuery<Result<PublicCatalogCategoryPage>>;
