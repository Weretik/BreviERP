namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingCustomCalculation(
    SewingCostBreakdown? Breakdown, SewingCustomCharges Charges, decimal? InternalCost,
    SewingPriceTiers? Prices, IReadOnlyList<SewingPricingBlocker> Blockers)
{
    public const string FormulaVersion = "custom-sewing-sheet-v1";
    public bool IsCalculated => Breakdown is not null && InternalCost is not null
        && Prices is not null && Blockers.Count == 0;
}
