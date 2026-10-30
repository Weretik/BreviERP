using Crm.Domain.CustomSewing.Composition;

namespace Crm.Domain.CustomSewing.Pricing;

public static class CustomCalculationReadiness
{
    public static bool IsValidTier(decimal? value) => value is > 0 and <= 999999999999999999.99m
        && decimal.Round(value.Value, 2) == value.Value;

    public static IReadOnlyList<string> Inspect(CustomConfiguration configuration,
        decimal? retail, decimal? wholesale, decimal? largeWholesale, bool artworkVerified = false)
    {
        var blockers = new List<string>();
        if (configuration.Applications.Any(x => !x.SaleApproved)) blockers.Add("ApplicationSaleNotApproved");
        if (!artworkVerified && configuration.Applications.Any(x => x.ArtworkFileId is not null))
            blockers.Add("ArtworkFileNotVerified");
        if (new[] { retail, wholesale, largeWholesale }.Any(value => value is not null && !IsValidTier(value)))
            blockers.Add("PriceOutOfRange");
        return blockers;
    }
}
