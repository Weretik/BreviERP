using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.GetOne;
using Catalog.Domain.Products.Enums;
using Content.Application.Abstractions;
using Host.Api.Crm;
using Mediator;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.Fabric.GetList.DTOs;

namespace Host.Api.Content;

/// <summary>
/// Gives Content the Catalog products through the Catalog and Reference queries: name, code, section of «Типи і
/// розділи», main photo, published prices and fabric variants. Content stores none of it.
/// </summary>
internal sealed class ContentCatalogReader(ISender sender, IProductSectionReader sections) : IContentCatalogReader
{
    public async Task<IReadOnlyList<ContentCatalogProduct>> GetAllAsync(CancellationToken cancellationToken)
    {
        var fabrics = await FabricsAsync(cancellationToken);
        var names = await SectionNamesAsync(cancellationToken);
        var products = new List<ContentCatalogProduct>();
        foreach (var type in new[] { ProductType.Sewing, ProductType.Ppe })
        {
            var count = 0;
            for (var page = 1; ; page++)
            {
                var result = await sender.Send(new GetProductOverviewPageQuery(type, null, null, false, false,
                    ProductOverviewSort.Code, false, page, GetProductOverviewPageQuery.MaxPageSize), cancellationToken);
                if (!result.IsSuccess) break;
                products.AddRange(result.Value.Items.Select(x => Map(x, fabrics, names)));
                count += result.Value.Items.Count;
                if (count >= result.Value.Total || result.Value.Items.Count == 0) break;
            }
        }

        return products;
    }

    public async Task<ContentCatalogProduct?> GetAsync(int productId, CancellationToken cancellationToken)
    {
        var item = await sender.Send(new GetProductOverviewItemQuery(productId), cancellationToken);
        if (!item.IsSuccess) return null;
        return Map(item.Value, await FabricsAsync(cancellationToken), await SectionNamesAsync(cancellationToken));
    }

    private async Task<IReadOnlyDictionary<int, FabricRowDTO>> FabricsAsync(CancellationToken cancellationToken) =>
        (await sender.Send(new GetAllFabricQuery(), cancellationToken)).Value?.ToDictionary(x => x.Id)
        ?? new Dictionary<int, FabricRowDTO>();

    private async Task<IReadOnlyDictionary<int, string>> SectionNamesAsync(CancellationToken cancellationToken) =>
        (await sections.ListAsync(cancellationToken)).ToDictionary(x => x.Id, x => x.Name);

    private static ContentCatalogProduct Map(ProductOverviewItem item, IReadOnlyDictionary<int, FabricRowDTO> fabrics,
        IReadOnlyDictionary<int, string> sectionNames)
    {
        string? sectionName = null;
        if (item.SectionId is { } sectionId) sectionNames.TryGetValue(sectionId, out sectionName);
        return new ContentCatalogProduct(item.Id, item.Type, item.Code, item.Name, item.SectionId, sectionName,
            item.IsActive, item.CurrentSiteUrl, item.MainPhotoFullUrl, Tiers(item.Prices),
            item.Variants.Select(x => Map(x, fabrics)).ToArray());
    }

    private static ContentCatalogVariant Map(ProductOverviewVariant variant,
        IReadOnlyDictionary<int, FabricRowDTO> fabrics)
    {
        FabricRowDTO? fabric = null;
        if (variant.FabricId is { } fabricId) fabrics.TryGetValue(fabricId, out fabric);
        return new ContentCatalogVariant(variant.ArticleId, variant.SaleSku, variant.Active,
            fabric?.Name ?? variant.FabricName, QuoteCatalogReader.Details(fabric), Tiers(variant.Prices));
    }

    private static ContentPriceTiers? Tiers(ProductPriceTiers? prices) =>
        prices is null ? null : new ContentPriceTiers(prices.Retail, prices.Wholesale, prices.LargeWholesale);
}
