using Catalog.Contracts.Products;
using Content.Application.Abstractions;
using Reference.Contracts.Fabrics;
using Reference.Contracts.ProductSections;

namespace Content.Infrastructure.Adapters.Catalog;

/// <summary>
/// Gives Content the Catalog products: name, code, section of «Типи і розділи», main photo, published prices and
/// fabric variants with Reference fabric details. Content stores none of it.
/// </summary>
internal sealed class ContentCatalogReader(
    ICatalogProductReader products,
    IFabricDirectory fabricDirectory,
    IProductSectionDirectory sections) : IContentCatalogReader
{
    public async Task<IReadOnlyList<ContentCatalogProduct>> GetAllAsync(CancellationToken cancellationToken)
    {
        var fabrics = await FabricsAsync(cancellationToken);
        var names = await SectionNamesAsync(cancellationToken);
        var all = new List<ContentCatalogProduct>();
        foreach (var kind in new[] { CatalogProductKind.Sewing, CatalogProductKind.Ppe })
            all.AddRange((await products.ListAsync(kind, null, cancellationToken)).Select(x => Map(x, fabrics, names)));
        return all;
    }

    public async Task<ContentCatalogProduct?> GetAsync(int productId, CancellationToken cancellationToken) =>
        await products.FindAsync(productId, cancellationToken) is { } product
            ? Map(product, await FabricsAsync(cancellationToken), await SectionNamesAsync(cancellationToken))
            : null;

    public async Task<IReadOnlyList<ContentCatalogSection>> GetSectionsAsync(CancellationToken cancellationToken) =>
        (await sections.ListAsync(cancellationToken))
        .Select(x => new ContentCatalogSection(x.Id, x.Type, x.Name, x.IsActive)).ToArray();

    public async Task<IReadOnlyList<ContentCatalogPhoto>> GetPhotosAsync(int productId,
        CancellationToken cancellationToken) =>
        (await products.PhotosAsync(productId, cancellationToken))
        .Select(x => new ContentCatalogPhoto(x.Url, x.ThumbnailUrl, x.IsMain)).ToArray();

    private async Task<IReadOnlyDictionary<int, FabricInfo>> FabricsAsync(CancellationToken cancellationToken) =>
        (await fabricDirectory.ListAsync(cancellationToken)).ToDictionary(x => x.Id);

    private async Task<IReadOnlyDictionary<int, string>> SectionNamesAsync(CancellationToken cancellationToken) =>
        (await sections.ListAsync(cancellationToken)).ToDictionary(x => x.Id, x => x.Name);

    private static ContentCatalogProduct Map(CatalogProduct item, IReadOnlyDictionary<int, FabricInfo> fabrics,
        IReadOnlyDictionary<int, string> sectionNames)
    {
        string? sectionName = null;
        if (item.SectionId is { } sectionId) sectionNames.TryGetValue(sectionId, out sectionName);
        return new ContentCatalogProduct(item.Id, item.Kind.ToString(), item.Code, item.Name, item.SectionId,
            sectionName, item.IsActive, item.CurrentSiteUrl, item.MainPhotoFullUrl, Tiers(item.Prices),
            item.Variants.Select(x => Map(x, fabrics)).ToArray(), item.MainPhotoUrl);
    }

    private static ContentCatalogVariant Map(CatalogVariant variant, IReadOnlyDictionary<int, FabricInfo> fabrics)
    {
        FabricInfo? fabric = null;
        if (variant.FabricId is { } fabricId) fabrics.TryGetValue(fabricId, out fabric);
        return new ContentCatalogVariant(variant.ArticleId, variant.SaleSku, variant.Active,
            fabric?.Name ?? variant.FabricName, fabric?.ClientDetails, Tiers(variant.Prices));
    }

    private static ContentPriceTiers? Tiers(CatalogPrices? prices) =>
        prices is null ? null : new ContentPriceTiers(prices.Retail, prices.Wholesale, prices.LargeWholesale);
}
