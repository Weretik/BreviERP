using Catalog.Domain.Pricing.Sewing;

namespace SewingPricing.Jobs.Verification;

public sealed record SewingPricingDiagnostic(
    string SaleSku, string FormulaVersion, long? SettingsVersionId, IReadOnlyList<long> ReferenceVersionIds,
    SewingCostBreakdown? ArithmeticBreakdown, SewingPriceTiers? ArithmeticPrices,
    IReadOnlyList<SewingPricingBlocker> StrictBlockers, IReadOnlyList<PricingParityDifference> Differences)
{
    public bool CanPublish => false;
}

