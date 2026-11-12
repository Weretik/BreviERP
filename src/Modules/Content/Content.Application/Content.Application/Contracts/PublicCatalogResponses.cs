using Content.Domain.ProductPages;

namespace Content.Application.Contracts;

/// <summary>The catalog tree of the site: the two product types and, under each, the sections that have products on the site.</summary>
public sealed record PublicCatalogTree(IReadOnlyList<PublicCatalogType> Types);

/// <param name="Type"><c>Sewing</c> or <c>Ppe</c>.</param>
/// <param name="ProductCount">Products of the type on the site, with and without a section.</param>
public sealed record PublicCatalogType(string Type, int ProductCount, IReadOnlyList<PublicCatalogSection> Sections);

/// <param name="Slug">Built from the Ukrainian name; the same for both languages.</param>
public sealed record PublicCatalogSection(int Id, string Slug, string Name, int ProductCount);

/// <summary>
/// A product in a listing. <paramref name="PriceFrom"/> is the lowest large-wholesale price; null means «ціну
/// уточнюйте». <paramref name="Fabrics"/> are the names of the active fabrics of a sewing model, each once, in the
/// order of the model (R-13); empty for PPE.
/// </summary>
public sealed record PublicCatalogCard(
    string Slug,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? PhotoUrl,
    decimal? PriceFrom,
    IReadOnlyList<string> Fabrics);

public sealed record PublicCatalogCardPage(IReadOnlyList<PublicCatalogCard> Items, int Page, int PageSize, int Total);

public sealed record PublicCatalogSectionLink(int Id, string Slug, string Name);

public sealed record PublicCatalogPhoto(string Url, string? ThumbnailUrl, bool IsMain);

/// <summary>A fabric of a sewing model with its three tier prices; null prices mean «ціну уточнюйте».</summary>
public sealed record PublicCatalogVariant(
    string Sku,
    string? FabricName,
    string? FabricDetails,
    ContentPriceTiersResponse? Prices);

/// <summary>
/// The category tree of the site (CNT-003 R-06): shown categories in their order, each with its shown subcategories.
/// </summary>
public sealed record PublicCatalogCategoryTree(IReadOnlyList<PublicCatalogCategory> Categories);

/// <param name="NameRu">The Russian name, or the Ukrainian one when there is none.</param>
/// <param name="ProductCount">Products on the site in the category and its subcategories, each counted once.</param>
public sealed record PublicCatalogCategory(
    string Slug,
    string NameUk,
    string NameRu,
    int ProductCount,
    IReadOnlyList<PublicCatalogCategory> Children);

public sealed record PublicCatalogCategoryLink(string Slug, string Name);

public sealed record PublicCatalogChildCategory(string Slug, string Name, int ProductCount);

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

/// <summary>A category the product is in, with its top-level category for the breadcrumbs.</summary>
public sealed record PublicCatalogProductCategory(string Slug, string Name, PublicCatalogCategoryLink? Parent);

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

/// <summary>An address for the sitemap. <paramref name="HasRussian"/> is false when the Russian page shows Ukrainian texts.</summary>
public sealed record PublicCatalogSlug(string Slug, DateTimeOffset UpdatedAtUtc, bool HasRussian);
