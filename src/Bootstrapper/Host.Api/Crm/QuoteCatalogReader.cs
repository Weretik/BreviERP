using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.GetOne;
using Catalog.Domain.Products.Enums;
using Crm.Application.Quotes.Catalog;
using Crm.Domain.Quotes;
using Mediator;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.Fabric.GetList.DTOs;

namespace Host.Api.Crm;

/// <summary>
/// Gives the КП editor Catalog products through the Catalog and Reference queries: published prices of fabric variants
/// (sewing) or the rule price (PPE), the «Короткий опис для КП» texts and the main photo.
/// </summary>
internal sealed class QuoteCatalogReader(ISender sender) : IQuoteCatalogReader
{
    public async Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, string? season, int page,
        int pageSize, CancellationToken cancellationToken)
    {
        var type = kind == QuoteItemKind.Sewing ? ProductType.Sewing : ProductType.Ppe;
        var fabrics = await FabricsAsync(cancellationToken);
        if (season is not null) return await SearchSeasonAsync(type, search, season, page, pageSize, fabrics, cancellationToken);

        var result = await sender.Send(OverviewPage(type, search, page, pageSize), cancellationToken);
        if (!result.IsSuccess) return new QuoteCatalogPage([], page, pageSize, 0);
        var items = result.Value.Items.Select(x => Map(x, fabrics)).ToArray();
        return new QuoteCatalogPage(items, result.Value.Page, result.Value.PageSize, result.Value.Total);
    }

    /// <summary>
    /// The Catalog list has no season filter: the introduction picker reads the whole list (about 130 models) and
    /// filters it here.
    /// </summary>
    private async Task<QuoteCatalogPage> SearchSeasonAsync(ProductType type, string? search, string season, int page,
        int pageSize, IReadOnlyDictionary<int, FabricRowDTO> fabrics, CancellationToken cancellationToken)
    {
        var rows = new List<ProductOverviewItem>();
        for (var next = 1; ; next++)
        {
            var result = await sender.Send(OverviewPage(type, search, next, GetProductOverviewPageQuery.MaxPageSize),
                cancellationToken);
            if (!result.IsSuccess) break;
            rows.AddRange(result.Value.Items);
            if (rows.Count >= result.Value.Total || result.Value.Items.Count == 0) break;
        }

        var matching = rows.Where(x => string.Equals(x.Season?.Trim(), season, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var items = matching.Skip((page - 1) * pageSize).Take(pageSize).Select(x => Map(x, fabrics)).ToArray();
        return new QuoteCatalogPage(items, page, pageSize, matching.Length);
    }

    private static GetProductOverviewPageQuery OverviewPage(ProductType type, string? search, int page, int pageSize) =>
        new(type, search, null, false, false, ProductOverviewSort.Code, false, page,
            Math.Min(pageSize, GetProductOverviewPageQuery.MaxPageSize));

    public async Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        var fabrics = await FabricsAsync(cancellationToken);
        var products = new Dictionary<int, QuoteCatalogProduct>(productIds.Count);
        foreach (var id in productIds)
        {
            var item = await sender.Send(new GetProductOverviewItemQuery(id), cancellationToken);
            if (item.IsSuccess) products[id] = Map(item.Value, fabrics);
        }

        return products;
    }

    private async Task<IReadOnlyDictionary<int, FabricRowDTO>> FabricsAsync(CancellationToken cancellationToken) =>
        (await sender.Send(new GetAllFabricQuery(), cancellationToken)).Value?.ToDictionary(x => x.Id)
        ?? new Dictionary<int, FabricRowDTO>();

    /// <summary>The main photo of the card tab «Фото» in full size (CAT-019).</summary>
    private static QuoteCatalogProduct Map(ProductOverviewItem item, IReadOnlyDictionary<int, FabricRowDTO> fabrics)
    {
        var kind = item.Type == nameof(ProductType.Ppe) ? QuoteItemKind.Ppe : QuoteItemKind.Sewing;
        return new QuoteCatalogProduct(item.Id, kind, item.Code, item.Name, item.Season, item.IsActive,
            item.CurrentSiteUrl, item.QuoteDescription, item.MainPhotoFullUrl, null, Tiers(item.Prices), item.Problems,
            item.Variants.Select(x => Map(x, fabrics)).ToArray());
    }

    private static QuoteCatalogVariant Map(ProductOverviewVariant variant, IReadOnlyDictionary<int, FabricRowDTO> fabrics)
    {
        FabricRowDTO? fabric = null;
        if (variant.FabricId is { } fabricId) fabrics.TryGetValue(fabricId, out fabric);
        var title = fabric?.Name ?? variant.FabricName ?? variant.SaleSku;
        return new QuoteCatalogVariant(variant.ArticleId, variant.SaleSku, variant.Active, variant.FabricId, title,
            Details(fabric), FabricInfo(fabric), Tiers(variant.Prices), variant.Problems);
    }

    /// <summary>What the client reads under the fabric: «Короткий опис для КП», else composition and density.</summary>
    internal static string? Details(FabricRowDTO? fabric)
    {
        if (fabric is null) return null;
        if (!string.IsNullOrWhiteSpace(fabric.ProposalDescription)) return fabric.ProposalDescription.Trim();
        var parts = new[] { fabric.Composition, fabric.Density is { Length: > 0 } density ? $"щільність {density}" : null }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var text = string.Join(", ", parts!);
        return text.Length == 0 ? null : text;
    }

    /// <summary>For the manager only: fabric price and supplier.</summary>
    private static string? FabricInfo(FabricRowDTO? fabric) =>
        fabric is null ? null : $"{fabric.Price:0.##} грн/м · {fabric.ProviderName}";

    private static QuoteTierPrices? Tiers(ProductPriceTiers? prices) =>
        prices is null ? null : new QuoteTierPrices(prices.Retail, prices.Wholesale, prices.LargeWholesale);
}
