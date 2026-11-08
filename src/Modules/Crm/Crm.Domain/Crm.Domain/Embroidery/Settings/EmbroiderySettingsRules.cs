namespace Crm.Domain.Embroidery.Settings;

/// <summary>Limits and invariants of the embroidery settings (EMB-001, R-03). Input validators show the same rules in Ukrainian.</summary>
public static class EmbroiderySettingsRules
{
    public const int MarkupCount = 2;

    /// <summary>The old calculator's «Опт», «Стандарт», «Терміново»: only its archive still has three.</summary>
    public const int LegacyMarkupCount = 3;
    public const int MinHoops = 1;
    public const int MaxHoops = 30;
    public const int MaxExpenses = 100;
    public const int NameMaxLength = 120;
    public const int IdMaxLength = 64;
    public const decimal MaxAmount = 1_000_000m;

    public static void EnsureValid(EmbroiderySettingsContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        Require(content.Markups.Count == MarkupCount, "Exactly two markups.");
        Require(content.Hoops.Count is >= MinHoops and <= MaxHoops, "1–30 hoops.");
        Require(content.Expenses.Count <= MaxExpenses, "At most 100 extra costs.");
        Require(content.Stabilizer.WidthCm > 0 && content.Fabric.WidthCm > 0, "Roll widths are positive.");
        Require(content.Stabilizer.PricePerMetre >= 0 && content.Fabric.PricePerMetre >= 0 &&
            content.FabricIncreasePercent >= 0, "Material values are not negative.");
        Require(content.Rates is { ThreadRate: >= 0, HoopingRate: >= 0, ProgramHourRate: >= 0, TaxPercent: >= 0 },
            "Rates are not negative.");
        Require(content.Markups.All(x => x.Percent >= 0 && !string.IsNullOrWhiteSpace(x.Name)), "Markups are named and not negative.");
        Require(content.Hoops.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Label) &&
            x.BlankWidthCm > 0 && x.BlankLengthCm > 0 && x.GlueRate is null or >= 0), "Hoops have an id, a label and a blank size.");
        Require(Unique(content.Hoops.Select(x => x.Id)), "Hoop ids are unique.");
        Require(content.Expenses.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name) &&
            x.Rate is null or >= 0 && Enum.IsDefined(x.Basis) && Enum.IsDefined(x.Mode)), "Extra costs are named and not negative.");
        Require(Unique(content.Expenses.Select(x => x.Id)), "Extra cost ids are unique.");
    }

    private static bool Unique(IEnumerable<string> ids)
    {
        var list = ids.ToList();
        return list.Distinct(StringComparer.Ordinal).Count() == list.Count;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new ArgumentException(message);
    }
}
