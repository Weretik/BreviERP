namespace Warehouse.Domain.Stock;

/// <summary>A reviewed group of «Тканини» codes that are one physical fabric on the shelf.</summary>
/// <param name="Code">The canonical code the stock shows.</param>
/// <param name="Codes">Every code of the group, the canonical one first.</param>
public sealed record FabricAliasGroup(string Code, IReadOnlyList<string> Codes, string Name, string Composition,
    string Density);

/// <summary>
/// The reviewed alias manifest of the old «Цех» (revision greta-023-067-sintepon-077-078-v2): FAB-067 is FAB-023 and
/// FAB-077 is FAB-078. Their balances are shown and spent as one pair per colour; stock lines keep their own codes.
/// No other code is ever joined by name, supplier or price.
/// </summary>
public static class FabricAliases
{
    public const string Revision = "greta-023-067-sintepon-077-078-v2";

    public static readonly IReadOnlyList<FabricAliasGroup> Groups =
    [
        new("FAB-023", ["FAB-023", "FAB-067"], "Тканина \"Грета\" Черкаси, ВВ", "53% бавовна, 47% поліестер", "220"),
        new("FAB-078", ["FAB-078", "FAB-077"], "Синтепон 100", "100% пе", "100")
    ];

    private static readonly Dictionary<string, FabricAliasGroup> ByCode =
        Groups.SelectMany(g => g.Codes.Select(c => (c, g))).ToDictionary(x => x.c, x => x.g, StringComparer.Ordinal);

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    /// <summary>The code the stock shows for a fabric code.</summary>
    public static string Canonical(string code)
    {
        var normalized = Normalize(code);
        return ByCode.TryGetValue(normalized, out var group) ? group.Code : normalized;
    }

    /// <summary>Every code that shares the shelf with this one, the canonical first.</summary>
    public static IReadOnlyList<string> Members(string code)
    {
        var normalized = Normalize(code);
        return ByCode.TryGetValue(normalized, out var group) ? group.Codes : [normalized];
    }

    public static FabricAliasGroup? Group(string code) => ByCode.GetValueOrDefault(Normalize(code));

    /// <summary>
    /// False when a code of a group no longer has the reviewed composition and density in «Тканини»: the offer changed
    /// and must not be taken for the same fabric silently.
    /// </summary>
    public static bool MatchesGroup(string code, string? composition, string? density)
    {
        var group = Group(code);
        if (group is null) return true;
        return Same(group.Composition, composition) && Number(group.Density) is { } expected &&
               Number(density) == expected;
    }

    private static bool Same(string expected, string? actual) =>
        Squash(expected) == Squash(actual ?? string.Empty);

    private static string Squash(string value) =>
        string.Concat(StockColor.Key(value).Where(c => !char.IsWhiteSpace(c))).Replace(',', '.');

    private static decimal? Number(string? value)
    {
        var text = new string((value ?? string.Empty).Where(c => char.IsDigit(c) || c is ',' or '.').ToArray())
            .Replace(',', '.');
        return decimal.TryParse(text, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var number) && number > 0 ? number : null;
    }
}
