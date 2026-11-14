using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared.Links;

namespace Content.Application.Features.Catalog.Shared.Products;

/// <summary>A category the product is in, with its top-level category for the breadcrumbs.</summary>
public sealed record PublicCatalogProductCategory(string Slug, string Name, PublicCatalogCategoryLink? Parent);
