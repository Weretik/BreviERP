namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingArticleCalculation(
    SewingCostBreakdown? Breakdown, SewingPriceTiers? Prices, IReadOnlyList<SewingPricingBlocker> Blockers)
{
    public const string FormulaVersion = "sewing-sheet-v1";
    public bool IsCalculated => Breakdown is not null && Prices is not null && Blockers.Count == 0;
}

