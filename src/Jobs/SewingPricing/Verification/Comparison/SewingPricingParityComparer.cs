using Catalog.Domain.Pricing.Sewing;
using CatalogImport.Source;

namespace SewingPricing.Jobs.Verification;

public static class SewingPricingParityComparer
{
    public static IReadOnlyList<PricingParityDifference> Compare(WorkbookRow row, SewingArticleCalculation calculation)
    {
        var actual = new decimal?[] { calculation.Breakdown?.MaterialsTotal, calculation.Breakdown?.Seamstress,
            calculation.Breakdown?.Cutting, calculation.Breakdown?.Master, calculation.Breakdown?.Manager,
            calculation.Breakdown?.Bonus, calculation.Breakdown?.Overhead, calculation.Breakdown?.FullCost,
            calculation.Prices?.Retail, calculation.Prices?.Wholesale, calculation.Prices?.LargeWholesale };
        var names = new[] { "MaterialsTotal", "Seamstress", "Cutting", "Master", "Manager", "Bonus", "Overhead",
            "FullCost", "Retail", "Wholesale", "LargeWholesale" };
        var differences = new List<PricingParityDifference>();
        for (var index = 0; index < actual.Length; index++)
        {
            var expected = row.Decimal(13 + index)
                ?? throw new InvalidDataException($"Missing source parity result at {row.Key}.");
            if (index >= 8)
                expected = decimal.Round(expected, 2, MidpointRounding.AwayFromZero);
            if (!actual[index].HasValue || Math.Abs(expected - actual[index]!.Value) > 0.00000001m)
                differences.Add(new(names[index], expected, actual[index]));
        }
        return differences;
    }
}

