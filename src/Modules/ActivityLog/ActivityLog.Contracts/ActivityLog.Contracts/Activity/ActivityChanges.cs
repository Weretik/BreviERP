using System.Globalization;

namespace ActivityLog.Contracts.Activity;

/// <summary>
/// Builds the changed fields of a description: a field is listed only when its display value differs (ACT-001 R-03).
/// </summary>
public sealed class ActivityChanges
{
    /// <summary>«1 200,50»: space between thousands, comma before the decimals (no ICU needed).</summary>
    private static readonly NumberFormatInfo Ukrainian = new() { NumberGroupSeparator = " ", NumberDecimalSeparator = "," };
    private readonly List<ActivityChangeItem> _items = [];

    public IReadOnlyList<ActivityChangeItem> Items => _items;

    public bool Any => _items.Count > 0;

    public ActivityChanges Add(string field, string? from, string? to)
    {
        var before = Clean(from);
        var after = Clean(to);
        if (!string.Equals(before, after, StringComparison.Ordinal)) _items.Add(new ActivityChangeItem(field, before, after));
        return this;
    }

    public ActivityChanges Add(string field, decimal? from, decimal? to, string? unit = null) =>
        Add(field, Number(from, unit), Number(to, unit));

    public ActivityChanges Add(string field, DateOnly? from, DateOnly? to) =>
        Add(field, from?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), to?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));

    public ActivityChanges Add(string field, bool from, bool to) => Add(field, from ? "так" : "ні", to ? "так" : "ні");

    /// <summary>A number as the admin shows it: «1 200,50», with an optional unit such as «грн» or «м».</summary>
    public static string? Number(decimal? value, string? unit = null) =>
        value is { } number
            ? number.ToString(number == decimal.Truncate(number) ? "#,0" : "#,0.00", Ukrainian)
              + (unit is null ? "" : " " + unit)
            : null;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
