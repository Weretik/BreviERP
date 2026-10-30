using System.Globalization;
using System.Text.RegularExpressions;
namespace Crm.Application.CustomSewing.GetSuggestions;

// Exact legacy source rule: lib/custom-sewing-suggestions.ts (six garment names, no arbitrary prefix matching).
public static class LegacySewingSuggestionType
{
    private static readonly string[] Types = ["Напівкомбінезон", "Комбінезон", "Куртка", "Штани", "Жилет", "Костюм"];
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");
    public static bool IsSupported(string value) => Types.Contains(value, StringComparer.Ordinal);
    public static string? FromModelName(string name)
    {
        var normalized = Regex.Replace(name.Trim().ToLower(Ukrainian), @"\s+", " ");
        return Types.FirstOrDefault(type => normalized == type.ToLower(Ukrainian)
            || normalized.StartsWith(type.ToLower(Ukrainian) + " ", StringComparison.Ordinal));
    }
    public static string? SourceSeason(string season) => season switch { "summer" => "Літо", "winter" => "Зима", _ => null };
}
