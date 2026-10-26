using Catalog.Domain.Pricing.Sewing;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Reading;
using SewingPricing.Jobs.Bootstrap;

namespace SewingPricing.Jobs.Review;

internal static class SettingsInputReviewReader
{
    public static async Task<PricingReviewSubject> ReadAsync(CatalogDbContext catalog, PricingBootstrapSource source,
        DateTimeOffset at, CancellationToken cancellationToken)
    {
        var settings = await new SewingPricingSettingsReader(catalog).ReadCandidatesAsync(at, cancellationToken);
        var current = settings.FirstOrDefault();
        return new("SewingSettings", "Settings", current?.Id.ToString(), PricingReviewComparison.Fields(
            new { Unambiguous = true, ValuesMatch = true, FormulaVersion = SewingArticleCalculation.FormulaVersion },
            current is null ? null : new { Unambiguous = settings.Count == 1 || settings[0].EffectiveFrom != settings[1].EffectiveFrom,
                ValuesMatch = current.Settings == SewingSettingsSourceMapper.Map(source.Settings), current.FormulaVersion }));
    }
}
