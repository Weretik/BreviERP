namespace Catalog.Contracts.Products;

/// <summary>A Catalog product as other modules see it: sewn (Sewing) or resold (Ppe).</summary>
public enum CatalogProductKind
{
    Sewing = 1,
    Ppe = 2
}

/// <summary>Published prices by quantity tier.</summary>
public sealed record CatalogPrices(decimal Retail, decimal Wholesale, decimal LargeWholesale);

/// <summary>
/// A product of «Товари» with its published prices and fabric variants. <paramref name="Problems"/> are the reasons
/// the product cannot be offered as is; <paramref name="MainPhotoFullUrl"/> is the main photo in full size.
/// </summary>
public sealed record CatalogProduct(
    int Id,
    CatalogProductKind Kind,
    string? Code,
    string Name,
    int? SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    CatalogPrices? Prices,
    IReadOnlyList<string> Problems,
    IReadOnlyList<CatalogVariant> Variants,
    string? MainPhotoUrl,
    string? MainPhotoFullUrl);

/// <summary>A fabric variant (article) of a sewn product, or the single article of a PPE product.</summary>
public sealed record CatalogVariant(
    long ArticleId,
    string SaleSku,
    bool Active,
    int? FabricId,
    string? FabricName,
    CatalogPrices? Prices,
    IReadOnlyList<string> Problems);

public sealed record CatalogProductPage(IReadOnlyList<CatalogProduct> Items, int Page, int PageSize, int Total);

/// <summary>A visible photo of the product card, main first.</summary>
public sealed record CatalogPhoto(string Url, string? ThumbnailUrl, bool IsMain);
