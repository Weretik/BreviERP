namespace Content.Application.Abstractions;

/// <summary>
/// What Content needs to know about products of other modules. Implemented in the host over Catalog and Reference
/// queries; Content keeps only the product id.
/// </summary>
public interface IContentCatalogReader
{
    /// <summary>Every Catalog product of both types, sewing first, by code.</summary>
    Task<IReadOnlyList<ContentCatalogProduct>> GetAllAsync(CancellationToken cancellationToken);

    Task<ContentCatalogProduct?> GetAsync(int productId, CancellationToken cancellationToken);

    /// <summary>Every section of «Типи і розділи» in the display order of the reference, sewing first.</summary>
    Task<IReadOnlyList<ContentCatalogSection>> GetSectionsAsync(CancellationToken cancellationToken);

    /// <summary>Photos of the card tab «Фото» that the library still has: the main one first, then by their order.</summary>
    Task<IReadOnlyList<ContentCatalogPhoto>> GetPhotosAsync(int productId, CancellationToken cancellationToken);
}

/// <summary>A section of «Типи і розділи». <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>.</summary>
public sealed record ContentCatalogSection(int Id, string Type, string Name, bool IsActive);

public sealed record ContentCatalogPhoto(string Url, string? ThumbnailUrl, bool IsMain);

/// <summary>
/// A Catalog product as the site sees it. <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>. <paramref name="Prices"/>
/// are the lowest tier prices among active variants (sewing) or the rule prices (PPE); null without a price.
/// <paramref name="MainPhotoPreviewUrl"/> is the small copy of the main photo for lists.
/// </summary>
public sealed record ContentCatalogProduct(
    int Id,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? SectionName,
    bool IsActive,
    string? OldSiteUrl,
    string? MainPhotoUrl,
    ContentPriceTiers? Prices,
    IReadOnlyList<ContentCatalogVariant> Variants,
    string? MainPhotoPreviewUrl = null);

/// <summary>A fabric variant of a sewing model with its published prices. PPE products have none.</summary>
public sealed record ContentCatalogVariant(
    long ArticleId,
    string SaleSku,
    bool IsActive,
    string? FabricName,
    string? FabricDetails,
    ContentPriceTiers? Prices);

public sealed record ContentPriceTiers(decimal Retail, decimal Wholesale, decimal LargeWholesale);
