namespace Crm.Domain.CustomSewing.Pricing;

public sealed record CustomPriceBasis(Guid Token, DateTimeOffset CalculatedAt, DateTimeOffset ExpiresAt,
    string FormulaVersion, long? SettingsVersionId, IReadOnlyList<CustomReferenceBasis> References,
    CustomCostBreakdown? Breakdown, decimal? InternalCost, decimal? Retail, decimal? Wholesale,
    decimal? LargeWholesale, IReadOnlyList<string> Blockers)
{
    public bool IsCalculated => Blockers.Count == 0 && Breakdown is not null && InternalCost is > 0
        && CustomCalculationReadiness.IsValidTier(Retail) && CustomCalculationReadiness.IsValidTier(Wholesale)
        && CustomCalculationReadiness.IsValidTier(LargeWholesale) && SettingsVersionId is > 0;

    public CustomPriceBasis Freeze() => this with {
        References = Array.AsReadOnly(References.ToArray()), Blockers = Array.AsReadOnly(Blockers.ToArray()) };
}
