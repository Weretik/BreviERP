using Catalog.Contracts.Products;
using Catalog.Domain.Products;
using Mediator;
using Catalog.Application.Features.Product.Queries.GetProductOverviewItem;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Queries.GetProductPhotos;
using Catalog.Domain.Products.Enums;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace Catalog.Infrastructure.Contracts.Products;

/// <summary>«Товари» for other modules through the Catalog product overview and photo queries.</summary>
internal sealed class CatalogProductReader(ISender sender) : ICatalogProductReader
{
    public async Task<CatalogProductPage> PageAsync(CatalogProductKind kind, string? search, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(Query(kind, search, page, Math.Min(pageSize, GetProductOverviewPageQuery.MaxPageSize)),
            cancellationToken);
        if (!result.IsSuccess) return new CatalogProductPage([], page, pageSize, 0);
        return new CatalogProductPage(result.Value.Items.Select(Map).ToArray(), result.Value.Page,
            result.Value.PageSize, result.Value.Total);
    }

    public async Task<IReadOnlyList<CatalogProduct>> ListAsync(CatalogProductKind kind, string? search,
        CancellationToken cancellationToken)
    {
        var rows = new List<CatalogProduct>();
        for (var page = 1; ; page++)
        {
            var result = await sender.Send(Query(kind, search, page, GetProductOverviewPageQuery.MaxPageSize),
                cancellationToken);
            if (!result.IsSuccess) throw new InvalidOperationException("Catalog products could not be read.");
            rows.AddRange(result.Value.Items.Select(Map));
            if (rows.Count >= result.Value.Total || result.Value.Items.Count == 0) break;
        }

        return rows;
    }

    public async Task<CatalogProduct?> FindAsync(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductOverviewItemQuery(productId), cancellationToken);
        return result.IsSuccess ? Map(result.Value) : null;
    }

    public async Task<IReadOnlyList<CatalogPhoto>> PhotosAsync(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductPhotosQuery(productId), cancellationToken);
        if (!result.IsSuccess) return [];
        return result.Value.Photos.Where(x => !x.Missing && x.Url is not null)
            .OrderByDescending(x => x.IsMain).ThenBy(x => x.SortOrder)
            .Select(x => new CatalogPhoto(x.Url!, x.ThumbnailUrl, x.IsMain)).ToArray();
    }

    private static GetProductOverviewPageQuery Query(CatalogProductKind kind, string? search, int page, int pageSize) =>
        new(kind == CatalogProductKind.Ppe ? ProductType.Ppe : ProductType.Sewing, search, null, false, false,
            ProductOverviewSort.Code, false, page, pageSize);

    private static CatalogProduct Map(ProductOverviewItem x) => new(x.Id,
        x.Type == nameof(ProductType.Ppe) ? CatalogProductKind.Ppe : CatalogProductKind.Sewing, x.Code, x.Name,
        x.SectionId, x.Season, x.IsActive, x.CurrentSiteUrl, x.QuoteDescription, Prices(x.Prices), x.Problems,
        x.Variants.Select(v => new CatalogVariant(v.ArticleId, v.SaleSku, v.Active, v.FabricId, v.FabricName,
            Prices(v.Prices), v.Problems)).ToArray(), x.MainPhotoUrl, x.MainPhotoFullUrl);

    private static CatalogPrices? Prices(ProductPriceTiers? prices) =>
        prices is null ? null : new CatalogPrices(prices.Retail, prices.Wholesale, prices.LargeWholesale);
}
