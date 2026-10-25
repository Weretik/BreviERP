using Catalog.Domain.Pricing.Ppe;
using CatalogImport.Source;

namespace CatalogImport.PpePricing.Verification;

public static class PpePricingParityComparer
{
    public static IReadOnlyList<PpePricingParityValue> Compare(WorkbookRow row, PpePriceTiers? prices)
    {
        var values = new decimal?[] { prices?.Retail, prices?.Wholesale, prices?.LargeWholesale };
        var names = new[] { "Retail", "Wholesale", "LargeWholesale" };
        var columns = new[] { "H", "I", "J" };
        var result = new List<PpePricingParityValue>();
        for (var index = 0; index < values.Length; index++)
        {
            decimal? expected = null;
            string? error = null;
            try { expected = row.Decimal(7 + index); }
            catch (InvalidDataException) { error = "InvalidSourceValue"; }
            if (expected.HasValue)
                expected = decimal.Round(expected.Value, 2, MidpointRounding.AwayFromZero);
            var difference = expected.HasValue && values[index].HasValue ? values[index] - expected : null;
            var status = error ?? (expected is null ? "MissingSourceValue" : values[index] is null
                ? "MissingCalculation" : Math.Abs(difference!.Value) > .00000001m ? "Different" : "Matched");
            result.Add(new(names[index], $"{row.Sheet}!{columns[index]}{row.Number}", row.Text(7 + index),
                expected, values[index], difference, status));
        }
        return result;
    }
}
