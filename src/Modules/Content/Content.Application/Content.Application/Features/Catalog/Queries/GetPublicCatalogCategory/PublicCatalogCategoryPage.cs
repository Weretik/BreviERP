using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;

/// <summary>
/// A category page of the site. <paramref name="Language"/> is the language of the texts that came back: a category
/// without Russian texts answers a Russian request with the Ukrainian ones.
/// </summary>
public sealed record PublicCatalogCategoryPage(
    string Slug,
    string Name,
    string Language,
    PublicCatalogCategoryLink? Parent,
    IReadOnlyList<PublicCatalogChildCategory> Children,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductTextBlock> Text,
    int ProductCount,
    DateTimeOffset UpdatedAtUtc);
