using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Catalog.Application.Features.Pricing.Sewing.Recalculation;
using Catalog.Application.Features.Product.Overview.Prices.DTOs;
using Catalog.Application.Features.Product.Overview.Prices.Specifications;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Prices;

public sealed class GetProductOverviewPricesQueryHandler(
    ICatalogReadRepository<ProductEntity> products,
    SewingPriceStatusReader sewing,
    IPpePricingInputReader ppe,
    TimeProvider timeProvider)
    : IQueryHandler<GetProductOverviewPricesQuery, Result<IReadOnlyList<ProductOverviewPrices>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductOverviewPrices>>> Handle(
        GetProductOverviewPricesQuery query, CancellationToken cancellationToken)
    {
        var ids = await products.ListAsync(new ProductIdsByTypeSpec(query.Type), cancellationToken);
        if (ids.Count == 0)
            return Result.Success<IReadOnlyList<ProductOverviewPrices>>([]);
        return query.Type == ProductType.Sewing
            ? Result.Success(Sewing(await sewing.ReadAsync(ids, timeProvider.GetUtcNow(), cancellationToken)))
            : Result.Success(Ppe(await ppe.ReadAsync(ids, cancellationToken)));
    }

    private static IReadOnlyList<ProductOverviewPrices> Sewing(IReadOnlyList<SewingModelPriceStatus> models)
        => models.Select(model =>
        {
            var active = model.Articles.Where(x => x.Active).ToList();
            var articles = model.Articles.Select(x => new ProductOverviewArticlePrices(
                x.ArticleId, x.SaleSku, x.Active, x.FabricId, Tiers(x.Published?.Prices), x.Published?.PublishedAt,
                Tiers(x.Live.Prices), x.Published is not null && x.NeedsPublication,
                x.Live.Blockers.Select(b => b.Code).Distinct().ToList())).ToList();
            var priced = articles.Where(x => x.Active && x.Published is not null).Select(x => x.Published!).ToList();
            var status = active.Count == 0 || active.All(x => !x.Live.IsCalculated && x.Published is null) ? "Blocked"
                : articles.Any(x => x.IsStale) ? "Stale"
                : active.Any(x => x.NeedsPublication) ? "Unpublished"
                : "Ready";
            return new ProductOverviewPrices(model.ProductId, status, Lowest(priced),
                active.Select(x => x.Published?.PublishedAt).Max(),
                active.Count == 0 ? ["NoActiveArticle"] : active.SelectMany(x => x.Live.Blockers).Select(b => b.Code).Distinct().ToList(),
                articles);
        }).ToList();

    private static IReadOnlyList<ProductOverviewPrices> Ppe(IReadOnlyList<PpePricingProductInput> inputs)
        => inputs.Select(PpeProductPricingEvaluator.Evaluate).Select(x =>
        {
            var prices = x.Calculation.IsCalculated ? x.Calculation.Prices : null;
            return new ProductOverviewPrices(x.ProductId, prices is null ? "Blocked" : "Ready",
                prices is null ? null : new ProductPriceTiers(prices.Retail, prices.Wholesale, prices.LargeWholesale),
                x.Offer?.CreatedAt, x.Calculation.Blockers.Select(b => b.Code).Distinct().ToList(), []);
        }).ToList();

    private static ProductPriceTiers? Tiers(SewingPriceTiers? prices)
        => prices is null ? null : new ProductPriceTiers(prices.Retail, prices.Wholesale, prices.LargeWholesale);

    private static ProductPriceTiers? Lowest(IReadOnlyList<ProductPriceTiers> prices)
        => prices.Count == 0 ? null
            : new ProductPriceTiers(prices.Min(x => x.Retail), prices.Min(x => x.Wholesale), prices.Min(x => x.LargeWholesale));
}
