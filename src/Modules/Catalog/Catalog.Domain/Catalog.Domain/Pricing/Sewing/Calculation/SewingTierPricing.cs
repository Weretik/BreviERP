namespace Catalog.Domain.Pricing.Sewing;

internal static class SewingTierPricing
{
    internal static SewingPriceTiers Calculate(decimal sewingCost, SewingCalculationSettings settings,
        decimal applicationSale = 0m) => new(
        Round(sewingCost * settings.RetailMultiplier + applicationSale),
        Round(sewingCost * settings.WholesaleMultiplier + applicationSale),
        Round(sewingCost * settings.LargeWholesaleMultiplier + applicationSale));

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
