using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.Catalog.Shared.Links;
using Content.Application.Features.ProductPages.Shared.Responses;

namespace Content.Application.Features.Catalog.Shared.Products;

/// <summary>
/// A product page of the site. <paramref name="Language"/> is the language of the texts that came back: a product
/// without Russian texts answers a Russian request with the Ukrainian ones. <paramref name="Prices"/> are the rule
/// prices of a PPE product or the lowest tier prices of a sewing model; <paramref name="Variants"/> are the active
/// fabrics of a sewing model. <paramref name="Categories"/> are the shown site categories of the product, in tree order.
/// </summary>
public sealed record PublicCatalogProduct(
    string Slug,
    string Type,
    string? Code,
    string Name,
    string Language,
    PublicCatalogSectionLink? Section,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductFact> Summary,
    IReadOnlyList<ProductTextBlock> Description,
    IReadOnlyList<ProductSpecGroup> Specs,
    IReadOnlyList<PublicCatalogPhoto> Photos,
    decimal? PriceFrom,
    ContentPriceTiersResponse? Prices,
    IReadOnlyList<PublicCatalogVariant> Variants,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<PublicCatalogProductCategory> Categories);
