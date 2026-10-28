namespace Catalog.Domain.Pricing.Sewing;

public static class SewingCustomCalculator
{
    public static SewingCustomCalculation Calculate(SewingCalculationSettings settings,
        IReadOnlyList<SewingMaterialCostInput> materials,
        IReadOnlyList<SewingOperationCostInput> operations, SewingCustomCharges charges)
    {
        var baseCalculation = SewingArticleCalculator.Calculate(settings, materials, operations);
        var blockers = new List<SewingPricingBlocker>(baseCalculation.Blockers);
        foreach (var (field, value, maximum) in new[]
        {
            ("delivery", charges.Delivery, 10000000m), ("packaging", charges.Packaging, 10000000m),
            // Aggregate of at most twenty application amounts, each bounded by the configuration contract.
            ("applicationCost", charges.ApplicationCost, 200000000m),
            ("applicationSale", charges.ApplicationSale, 200000000m)
        })
        {
            if (value < 0 || value > maximum || decimal.Round(value, 2) != value)
                blockers.Add(new("InvalidCustomCharge", field,
                    "Custom charges must be nonnegative two-place amounts within the supported configuration bounds."));
        }
        if (blockers.Count != 0)
            return new(null, charges, null, null, blockers);
        try
        {
            var breakdown = baseCalculation.Breakdown!;
            var sewingCost = breakdown.FullCost + charges.Delivery + charges.Packaging;
            var internalCost = sewingCost + charges.ApplicationCost;
            var tiers = SewingTierPricing.Calculate(sewingCost, settings, charges.ApplicationSale);
            return new(breakdown, charges, internalCost, tiers, []);
        }
        catch (OverflowException)
        {
            return new(null, charges, null, null,
                [new("NumericOverflow", null, "Calculation exceeds decimal precision.")]);
        }
    }
}
