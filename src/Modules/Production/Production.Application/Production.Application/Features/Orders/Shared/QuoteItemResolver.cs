using Production.Application.Abstractions;
using Production.Domain.FabricNorms;
using Production.Domain.Orders;

namespace Production.Application.Features.Orders.Shared;

/// <summary>
/// Turns the КП items into what the workshop copies: model and fabric codes, the sewing rate of the fabric variant and
/// the workshop norm. Catalog costs and norms are read once per model.
/// </summary>
public sealed class QuoteItemResolver(IProductionDbContext db, IProductionModelReader models,
    IProductionFabricReader fabrics)
{
    private readonly Dictionary<int, ProductionModelCosts?> _costs = [];
    private Dictionary<int, ProductionFabric>? _fabrics;

    public async Task<ResolvedQuoteItem> ResolveAsync(ProductionQuoteItem item, CancellationToken cancellationToken)
    {
        ProductionModelCosts? costs = null;
        if (item.ProductId is { } productId)
        {
            if (!_costs.TryGetValue(productId, out costs))
                _costs[productId] = costs = await models.ReadCostsAsync(productId, cancellationToken);
        }

        _fabrics ??= (await fabrics.ListAsync(cancellationToken)).ToDictionary(x => x.Id);
        var code = costs?.ModelCode ?? (item.ProductId is null ? null : item.Code);
        var normalized = string.IsNullOrWhiteSpace(code) ? null : ModelFabricNorm.NormalizeCode(code);
        var norm = normalized is null
            ? null
            : await db.FabricNorms.AsNoTracking().FirstOrDefaultAsync(x => x.ModelCode == normalized, cancellationToken);
        var options = item.Options.Select(option =>
        {
            var fabric = option.FabricId is { } id ? _fabrics.GetValueOrDefault(id) : null;
            decimal? rate = option.ArticleId is { } article && costs is not null
                ? costs.SewingRates.GetValueOrDefault(article)
                : null;
            return new ResolvedQuoteOption(option, fabric?.Code, fabric?.Name ?? option.Title,
                rate is { } value ? decimal.Round(value, 2) : null);
        }).ToArray();
        return new ResolvedQuoteItem(item, normalized, norm?.Values, options);
    }
}

public sealed record ResolvedQuoteItem(
    ProductionQuoteItem Item, string? ModelCode, FabricNormValues? Norm, IReadOnlyList<ResolvedQuoteOption> Options);

public sealed record ResolvedQuoteOption(ProductionQuoteOption Option, string? FabricCode, string FabricName,
    decimal? SewingRate)
{
    public decimal? Price(ReportPriceTier tier) => tier switch
    {
        ReportPriceTier.Retail => Option.Retail,
        ReportPriceTier.Wholesale => Option.Wholesale,
        _ => Option.Large
    };
}
