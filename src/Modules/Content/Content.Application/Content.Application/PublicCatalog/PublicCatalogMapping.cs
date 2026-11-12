using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;

namespace Content.Application.PublicCatalog;

/// <summary>Turns a product of the site into what the public API returns. Nothing internal passes through (CNT-002 R-07).</summary>
internal static class PublicCatalogMapping
{
    /// <summary>
    /// The texts for a language: its own when the language has a name or texts, otherwise the Ukrainian ones. Null
    /// when the page has no texts at all.
    /// </summary>
    public static ProductPageText? TextFor(ProductPage page, string language)
    {
        var own = page.TextIn(language);
        return own is { IsEmpty: false } && (own.Name is not null || own.HasText)
            ? own
            : page.TextIn(ProductPageRules.Ukrainian);
    }

    public static bool HasRussian(ProductPage page) =>
        TextFor(page, ProductPageRules.Russian)?.Language == ProductPageRules.Russian;

    public static string Name(PublicCatalogEntry entry, string language) =>
        TextFor(entry.Page, language)?.Name
        ?? entry.Page.TextIn(ProductPageRules.Ukrainian)?.Name
        ?? entry.Product.Name;

    /// <summary>The lowest large-wholesale price among active fabrics, or the PPE rule price (R-08).</summary>
    public static decimal? PriceFrom(ContentCatalogProduct product)
    {
        var prices = product.Variants.Where(x => x is { IsActive: true, Prices: not null })
            .Select(x => x.Prices!.LargeWholesale).ToArray();
        return prices.Length > 0 ? prices.Min() : product.Prices?.LargeWholesale;
    }

    public static PublicCatalogCard Card(PublicCatalogEntry entry, string language) =>
        new(entry.Page.Slug, entry.Product.Type, entry.Product.Code, Name(entry, language), entry.Product.SectionId,
            entry.Product.MainPhotoPreviewUrl ?? entry.Product.MainPhotoUrl, PriceFrom(entry.Product),
            Fabrics(entry.Product));

    /// <summary>Names of the active fabrics, each once, in the order of the model (R-13).</summary>
    public static IReadOnlyList<string> Fabrics(ContentCatalogProduct product) =>
        product.Variants.Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.FabricName))
            .Select(x => x.FabricName!.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>The texts of a category for a language, with the same fallback as a product page.</summary>
    public static SiteCategoryText? TextFor(SiteCategory category, string language)
    {
        var own = category.TextIn(language);
        return own is not null && (own.Name is not null || own.Text.Count > 0)
            ? own
            : category.TextIn(ProductPageRules.Ukrainian);
    }

    public static bool HasRussian(SiteCategory category) =>
        TextFor(category, ProductPageRules.Russian)?.Language == ProductPageRules.Russian;

    public static string Name(SiteCategory category, string language) =>
        TextFor(category, language)?.Name
        ?? category.TextIn(ProductPageRules.Ukrainian)?.Name
        ?? category.Slug;

    public static PublicCatalogCategoryLink Link(SiteCategory category, string language) =>
        new(category.Slug, Name(category, language));

    /// <summary>Shown categories the product was put into, in tree order, each with its top-level category.</summary>
    public static IReadOnlyList<PublicCatalogProductCategory> CategoriesOf(PublicCatalogSnapshot snapshot,
        int productId, string language) =>
        snapshot.Categories.Where(x => x.OwnProductIds.Contains(productId))
            .Select(x => new PublicCatalogProductCategory(x.Category.Slug, Name(x.Category, language),
                snapshot.Categories.FirstOrDefault(parent => parent.Category.Id == x.Category.ParentId) is { } parent
                    ? Link(parent.Category, language)
                    : null))
            .ToArray();

    public static PublicCatalogProduct Product(PublicCatalogEntry entry, string language,
        PublicCatalogSnapshotSection? section, IReadOnlyList<ContentCatalogPhoto> photos,
        IReadOnlyList<PublicCatalogProductCategory> categories)
    {
        var text = TextFor(entry.Page, language);
        var product = entry.Product;
        return new PublicCatalogProduct(entry.Page.Slug, product.Type, product.Code, Name(entry, language),
            text?.Language == language ? language : ProductPageRules.Ukrainian,
            section is null ? null : new PublicCatalogSectionLink(section.Id, section.Slug, section.Name),
            text?.SeoTitle, text?.SeoDescription, text?.Summary ?? [], text?.Description ?? [], text?.Specs ?? [],
            photos.Select(x => new PublicCatalogPhoto(x.Url, x.ThumbnailUrl, x.IsMain)).ToArray(),
            PriceFrom(product), Tiers(product.Prices),
            product.Variants.Where(x => x.IsActive)
                .Select(x => new PublicCatalogVariant(x.SaleSku, x.FabricName, x.FabricDetails, Tiers(x.Prices)))
                .ToArray(),
            entry.Page.UpdatedAtUtc, categories);
    }

    private static ContentPriceTiersResponse? Tiers(ContentPriceTiers? prices) =>
        prices is null ? null : new ContentPriceTiersResponse(prices.Retail, prices.Wholesale, prices.LargeWholesale);
}
