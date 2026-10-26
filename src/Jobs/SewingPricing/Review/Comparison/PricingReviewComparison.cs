using System.Globalization;
using System.Text.Json;

namespace SewingPricing.Jobs.Review;

public static class PricingReviewComparison
{
    public static IReadOnlyList<PricingReviewCheck> Fields(object expected, object? actual)
    {
        var checks = new List<PricingReviewCheck> { new("Exists", "True", (actual is not null).ToString()) };
        Compare(JsonSerializer.SerializeToElement(expected), JsonSerializer.SerializeToElement(actual), "", checks);
        return checks;
    }

    private static void Compare(JsonElement expected, JsonElement actual, string prefix, List<PricingReviewCheck> checks)
    {
        foreach (var property in expected.EnumerateObject())
        {
            var field = prefix + property.Name;
            var value = actual.ValueKind == JsonValueKind.Object && actual.TryGetProperty(property.Name, out var found) ? found : default;
            if (property.Value.ValueKind == JsonValueKind.Object)
                Compare(property.Value, value, field + ".", checks);
            else
                checks.Add(new(field, Text(property.Value), Text(value)));
        }
    }

    private static string? Text(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Undefined => "<missing>",
        JsonValueKind.Null => null,
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetDecimal().ToString("G29", CultureInfo.InvariantCulture),
        _ => value.GetRawText()
    };
}
