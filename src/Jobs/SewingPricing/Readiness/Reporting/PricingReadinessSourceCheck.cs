using CatalogImport.Source;

namespace SewingPricing.Jobs.Readiness;

public static class PricingReadinessSourceCheck
{
    public static IReadOnlyList<string> Inspect(WorkbookRow row)
    {
        var ppe = row.Text(5) == "ЗІЗ";
        var offset = ppe ? 7 : 21;
        var columns = ppe ? new[] { "H", "I", "J" } : ["V", "W", "X"];
        var issues = new List<string>();
        for (var index = 0; index < columns.Length; index++)
        {
            var cell = $"{row.Sheet}!{columns[index]}{row.Number}";
            try
            {
                if (row.Decimal(offset + index) is null)
                    issues.Add($"MissingSourceValue:{cell}");
            }
            catch (InvalidDataException)
            {
                issues.Add($"InvalidSourceValue:{cell}");
            }
        }
        return issues;
    }
}
