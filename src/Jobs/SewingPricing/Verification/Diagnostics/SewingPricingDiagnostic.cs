using Catalog.Domain.Pricing.Sewing;

namespace SewingPricing.Jobs.Verification;

public sealed record SewingPricingDiagnostic(
    string SaleSku, string FormulaVersion, long? SettingsVersionId, IReadOnlyList<long> ReferenceVersionIds,
    SewingCostBreakdown? ArithmeticBreakdown, SewingPriceTiers? ArithmeticPrices,
    IReadOnlyList<SewingPricingBlocker> StrictBlockers, IReadOnlyList<PricingParityDifference> Differences, IReadOnlyList<SewingPricingParityValue> Comparisons)
{
    public string SourceHash { get; init; } = string.Empty;
    public string SpreadsheetId { get; init; } = string.Empty;
    public string SourceCapturedAt { get; init; } = string.Empty;
    public bool CanPublish => false;
}

