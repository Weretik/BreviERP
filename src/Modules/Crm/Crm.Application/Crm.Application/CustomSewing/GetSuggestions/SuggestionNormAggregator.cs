using Crm.Application.CustomSewing.GetSuggestions.Contracts;
using System.Text.Json;
namespace Crm.Application.CustomSewing.GetSuggestions;

public static class SuggestionNormAggregator
{
    public static SewingSuggestions Aggregate(string productType, string season, SuggestionSource source)
    {
        // Exact identifiers only. Conflicting variants cannot masquerade as a unique approved model.
        var models = source.Models.Where(x => x.ProductType == productType && x.Season == season
                && !string.IsNullOrWhiteSpace(x.ModelCode))
            .GroupBy(x => x.ModelCode, StringComparer.Ordinal)
            .Where(group => group.All(x => x.Approved && Valid(x)) && group.Select(x => Fingerprint(x.Selections)).Distinct(StringComparer.Ordinal).Count() == 1)
            .Select(group => group.First()).ToArray();
        var minimum = Math.Max(2, (int)Math.Ceiling(models.Length * .7));
        var norms = models.SelectMany(x => x.Selections)
            .GroupBy(x => new { x.Kind, x.ReferenceId, x.Unit, x.ManualMinutes })
            .Where(group => group.Count() >= minimum)
            .Select(group => BuildNorm(group.Key.Kind, group.Key.ReferenceId, group.Key.Unit, group.Key.ManualMinutes, group))
            .OrderBy(x => x.Kind, StringComparer.Ordinal).ThenBy(x => x.ReferenceId).ThenBy(x => x.Unit, StringComparer.Ordinal)
            .ThenBy(x => x.ManualMinutes).ToArray();
        return new(productType, season, models.Length, norms, source.LimitReached);
    }
    private static SuggestionNorm BuildNorm(string kind, int referenceId, string unit, decimal? manualMinutes,
        IEnumerable<SuggestionSelection> selections)
    {
        var quantities = selections.Select(x => x.Quantity).Order().ToArray();
        var median = quantities[quantities.Length / 2];
        return new(kind, referenceId, unit, manualMinutes, quantities.Length, median, quantities[0], quantities[^1],
            quantities.Any(x => Math.Abs(x - median) > .0001m));
    }
    private static bool Valid(SuggestionInput model) => model.Selections.Count > 0
        && model.Selections.All(x => x.ReferenceId > 0 && x.Kind is "accessory" or "operation"
            && !string.IsNullOrWhiteSpace(x.Unit) && x.Quantity > 0 && x.Quantity <= 100000
            && decimal.Round(x.Quantity, 6) == x.Quantity && (x.ManualMinutes is null or > 0))
        && model.Selections.Select(x => new { x.Kind, x.ReferenceId }).Distinct().Count() == model.Selections.Count;
    private static string Fingerprint(IReadOnlyList<SuggestionSelection> selections)
        => JsonSerializer.Serialize(selections.OrderBy(x => x.Kind, StringComparer.Ordinal).ThenBy(x => x.ReferenceId));
}
