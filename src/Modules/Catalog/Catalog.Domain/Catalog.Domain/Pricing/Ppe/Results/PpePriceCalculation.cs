namespace Catalog.Domain.Pricing.Ppe;
public sealed record PpePriceCalculation(PpePriceTiers? Prices, IReadOnlyList<PpePricingBlocker> Blockers)
{
    public bool IsCalculated => Prices is not null && Blockers.Count == 0;
}
