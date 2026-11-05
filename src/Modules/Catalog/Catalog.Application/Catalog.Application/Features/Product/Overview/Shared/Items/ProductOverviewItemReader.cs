using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Persistence.ProductOverview;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Catalog.Application.Features.Pricing.Sewing.AutoPublication;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Overview.GetList.Specifications;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Shared.Items;

/// <summary>
/// Builds «Товари» rows with their prices from a constant number of queries (CAT-013): sewing prices are the stored
/// published prices (automatic publication keeps them current, CAT-012), so nothing is recalculated for the list.
/// Only a sewing row with an unpriced active variant is evaluated, to say why it has no price.
/// </summary>
public sealed class ProductOverviewItemReader(
    ICatalogReadRepository<ProductEntity> products,
    IProductOverviewVariantReader variants,
    IFabricNameReader fabrics,
    IPpePricingInputReader ppe,
    SewingPriceStatusReader sewingStatus,
    IProductMediaReader media,
    TimeProvider timeProvider)
{
    public const string NoActiveVariant = "NoActiveArticle";
    public const string NotPublished = "Unpublished";

    /// <summary>Every row of the type, or the one product, with prices; sewing problems are not filled yet.</summary>
    public async Task<IReadOnlyList<ProductOverviewItem>> ReadAsync(
        ProductType? type, int? id, CancellationToken cancellationToken)
    {
        var rows = await products.ListAsync(new ProductOverviewSpec(type, id), cancellationToken);
        var sewingIds = rows.Where(x => x.Type == ProductType.Sewing).Select(x => x.Id).ToArray();
        var ppeIds = rows.Where(x => x.Type == ProductType.Ppe).Select(x => x.Id).ToArray();

        var variantRows = sewingIds.Length == 0 ? [] : await variants.ReadAsync(sewingIds, cancellationToken);
        var names = variantRows.Any(x => x.FabricId is not null)
            ? await fabrics.ReadAsync(cancellationToken)
            : new Dictionary<int, string>();
        var byProduct = variantRows.ToLookup(x => x.ProductId);
        var ppePrices = ppeIds.Length == 0
            ? new Dictionary<int, PpePrice>()
            : (await ppe.ReadAsync(ppeIds, cancellationToken)).Select(PpeProductPricingEvaluator.Evaluate)
                .ToDictionary(x => x.ProductId, x => new PpePrice(
                    x.Calculation.IsCalculated && x.Calculation.Prices is { } p
                        ? new ProductPriceTiers(p.Retail, p.Wholesale, p.LargeWholesale) : null,
                    x.Offer?.CreatedAt,
                    x.Calculation.Blockers.Select(b => b.Code).Distinct().ToList()));

        var photoIds = rows.Where(x => x.MainPhotoFileId is not null).Select(x => x.MainPhotoFileId!.Value).ToList();
        var photos = photoIds.Count == 0
            ? new Dictionary<int, ProductMediaFile>()
            : await media.GetFilesAsync(photoIds, cancellationToken);

        return rows.Select(row =>
        {
            var file = row.MainPhotoFileId is { } fileId ? photos.GetValueOrDefault(fileId) : null;
            return (row.Type == ProductType.Ppe
                ? Ppe(row, ppePrices.GetValueOrDefault(row.Id))
                : Sewing(row, byProduct[row.Id], names)) with
            {
                MainPhotoUrl = file is null ? null : file.ThumbnailUrl ?? file.PublicUrl,
                MainPhotoFullUrl = file?.PublicUrl
            };
        }).ToList();
    }

    /// <summary>Fills the reasons of sewing rows that have an active variant without a price.</summary>
    public async Task<IReadOnlyList<ProductOverviewItem>> WithProblemsAsync(
        IReadOnlyList<ProductOverviewItem> items, CancellationToken cancellationToken)
    {
        var unpriced = items.Where(x => x.Type == ProductOverviewTypes.Name(ProductType.Sewing)
            && x.Variants.Any(v => v.Active && v.Prices is null)).Select(x => x.Id).ToArray();
        if (unpriced.Length == 0)
            return items;
        var statuses = (await sewingStatus.ReadAsync(unpriced, timeProvider.GetUtcNow(), cancellationToken))
            .SelectMany(model => model.Articles).ToDictionary(x => x.ArticleId);
        return items.Select(item =>
        {
            if (!unpriced.Contains(item.Id))
                return item;
            var variantList = item.Variants.Select(variant =>
            {
                if (!variant.Active || variant.Prices is not null)
                    return variant;
                var codes = statuses.TryGetValue(variant.ArticleId, out var status)
                    ? status.Live.Blockers.Select(b => b.Code).Distinct().ToList()
                    : [];
                return variant with { Problems = codes.Count == 0 ? [NotPublished] : codes };
            }).ToList();
            return item with
            {
                Variants = variantList,
                Problems = variantList.SelectMany(v => v.Problems).Distinct().ToList()
            };
        }).ToList();
    }

    private static ProductOverviewItem Sewing(
        ProductOverviewReadModel row, IEnumerable<ProductOverviewVariantRow> variantRows,
        IReadOnlyDictionary<int, string> names)
    {
        var variantList = variantRows.OrderBy(x => x.SortOrder).ThenBy(x => x.ArticleId).Select(x =>
            new ProductOverviewVariant(x.ArticleId, x.SaleSku, x.IsActive, x.FabricId,
                x.FabricId is { } fabricId ? names.GetValueOrDefault(fabricId) : null,
                x.Retail is { } retail && x.Wholesale is { } wholesale && x.LargeWholesale is { } large
                    ? new ProductPriceTiers(retail, wholesale, large) : null,
                x.PublishedAt, [])).ToList();
        var active = variantList.Where(x => x.Active).ToList();
        var priced = active.Where(x => x.Prices is not null).Select(x => x.Prices!).ToList();
        return Item(row,
            priced.Count == 0 ? null : new ProductPriceTiers(priced.Min(x => x.Retail), priced.Min(x => x.Wholesale),
                priced.Min(x => x.LargeWholesale)),
            active.Select(x => x.PublishedAt).Max(),
            active.Count == 0 ? [NoActiveVariant] : [],
            variantList);
    }

    private static ProductOverviewItem Ppe(ProductOverviewReadModel row, PpePrice? price)
        => Item(row, price?.Prices, price?.UpdatedAt,
            price is null ? ["MissingOffer"] : price.Prices is null ? price.Blockers : [], []);

    private static ProductOverviewItem Item(
        ProductOverviewReadModel x, ProductPriceTiers? prices, DateTimeOffset? pricesUpdatedAt,
        IReadOnlyList<string> problems, IReadOnlyList<ProductOverviewVariant> variantList)
        => new(x.Id, ProductOverviewTypes.Name(x.Type), x.Type == ProductType.Ppe ? x.SaleSku : x.ModelCode, x.Name,
            x.SectionId, x.Season, x.IsActive, x.CurrentSiteUrl, x.QuoteDescription,
            x.UpdatedAt ?? x.CreatedAt, prices, pricesUpdatedAt, problems, variantList);

    private sealed record PpePrice(ProductPriceTiers? Prices, DateTimeOffset? UpdatedAt, IReadOnlyList<string> Blockers);
}
