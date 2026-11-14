using Catalog.Contracts.Products;
using Crm.Application.Quotes.Catalog;
using Crm.Domain.Quotes;
using Reference.Contracts.Fabrics;

namespace Crm.Infrastructure.Adapters.Catalog;

/// <summary>
/// Gives the КП editor Catalog products: published prices of fabric variants (sewing) or the rule price (PPE), the
/// «Короткий опис для КП» texts and the main photo, with Reference fabric details.
/// </summary>
internal sealed class QuoteCatalogReader(ICatalogProductReader products, IFabricDirectory fabricDirectory)
    : IQuoteCatalogReader
{
    public async Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page,
        int pageSize, CancellationToken cancellationToken)
    {
        var type = kind == QuoteItemKind.Sewing ? CatalogProductKind.Sewing : CatalogProductKind.Ppe;
        var fabrics = await FabricsAsync(cancellationToken);
        if (season is not null)
        {
            // The Catalog list has no season filter: the introduction picker reads the whole list (about 130 models).
            var matching = (await products.ListAsync(type, search, cancellationToken))
                .Where(x => string.Equals(x.Season?.Trim(), season, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var items = matching.Skip((page - 1) * pageSize).Take(pageSize).Select(x => Map(x, fabrics)).ToArray();
            return new QuoteCatalogPage(items, page, pageSize, matching.Length);
        }

        var result = await products.PageAsync(type, search, page, pageSize, cancellationToken);
        return new QuoteCatalogPage(result.Items.Select(x => Map(x, fabrics)).ToArray(), result.Page, result.PageSize,
            result.Total);
    }

    public async Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        var fabrics = await FabricsAsync(cancellationToken);
        var found = new Dictionary<int, QuoteCatalogProduct>(productIds.Count);
        foreach (var id in productIds)
            if (await products.FindAsync(id, cancellationToken) is { } product) found[id] = Map(product, fabrics);
        return found;
    }

    private async Task<IReadOnlyDictionary<int, FabricInfo>> FabricsAsync(CancellationToken cancellationToken) =>
        (await fabricDirectory.ListAsync(cancellationToken)).ToDictionary(x => x.Id);

    /// <summary>The main photo of the card tab «Фото» in full size (CAT-019).</summary>
    private static QuoteCatalogProduct Map(CatalogProduct item, IReadOnlyDictionary<int, FabricInfo> fabrics) =>
        new(item.Id, item.Kind == CatalogProductKind.Ppe ? QuoteItemKind.Ppe : QuoteItemKind.Sewing, item.Code,
            item.Name, item.Season, item.IsActive, item.CurrentSiteUrl, item.QuoteDescription, item.MainPhotoFullUrl,
            null, Tiers(item.Prices), item.Problems, item.Variants.Select(x => Map(x, fabrics)).ToArray());

    private static QuoteCatalogVariant Map(CatalogVariant variant, IReadOnlyDictionary<int, FabricInfo> fabrics)
    {
        FabricInfo? fabric = null;
        if (variant.FabricId is { } fabricId) fabrics.TryGetValue(fabricId, out fabric);
        var title = fabric?.Name ?? variant.FabricName ?? variant.SaleSku;
        return new QuoteCatalogVariant(variant.ArticleId, variant.SaleSku, variant.Active, variant.FabricId, title,
            fabric?.ClientDetails, FabricInfoText(fabric), Tiers(variant.Prices), variant.Problems);
    }

    /// <summary>For the manager only: fabric price and supplier.</summary>
    private static string? FabricInfoText(FabricInfo? fabric) =>
        fabric is null ? null : $"{fabric.Price:0.##} грн/м · {fabric.ProviderName}";

    private static QuoteTierPrices? Tiers(CatalogPrices? prices) =>
        prices is null ? null : new QuoteTierPrices(prices.Retail, prices.Wholesale, prices.LargeWholesale);
}
