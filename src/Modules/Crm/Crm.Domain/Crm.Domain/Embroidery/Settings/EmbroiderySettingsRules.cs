using BuildingBlocks.Domain.Results;

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

    /// <summary>The first broken rule of <paramref name="content"/>, or success.</summary>
    public static DomainResult Check(EmbroiderySettingsContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return DomainResult.Combine(
            Require(content.Markups.Count == MarkupCount, "markups", "Має бути рівно дві націнки."),
            Require(content.Hoops.Count is >= MinHoops and <= MaxHoops, "hoops", $"П'яльців — від {MinHoops} до {MaxHoops}."),
            Require(content.Expenses.Count <= MaxExpenses, "expenses", $"Додаткових витрат — не більше {MaxExpenses}."),
            Require(content.Stabilizer.WidthCm > 0 && content.Fabric.WidthCm > 0, "stabilizer.widthCm",
                "Ширина рулону має бути більшою за нуль."),
            Require(content.Stabilizer.PricePerMetre >= 0 && content.Fabric.PricePerMetre >= 0 &&
                content.FabricIncreasePercent >= 0, "fabric", "Ціни й відсоток матеріалів не можуть бути від'ємними."),
            Require(content.Rates is { ThreadRate: >= 0, HoopingRate: >= 0, ProgramHourRate: >= 0, TaxPercent: >= 0 },
                "rates", "Ставки не можуть бути від'ємними."),
            Require(content.Markups.All(x => x.Percent >= 0 && !string.IsNullOrWhiteSpace(x.Name)), "markups",
                "Кожна націнка має назву й невід'ємний відсоток."),
            Require(content.Hoops.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Label) &&
                x.BlankWidthCm > 0 && x.BlankLengthCm > 0 && x.GlueRate is null or >= 0), "hoops",
                "Кожен п'яльць має назву й розмір заготовки."),
            Require(Unique(content.Hoops.Select(x => x.Id)), "hoops", "П'яльці не повторюються."),
            Require(content.Expenses.All(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.Name) &&
                x.Rate is null or >= 0 && Enum.IsDefined(x.Basis) && Enum.IsDefined(x.Mode)), "expenses",
                "Кожна додаткова витрата має назву й невід'ємну ставку."),
            Require(Unique(content.Expenses.Select(x => x.Id)), "expenses", "Додаткові витрати не повторюються."));
    }

    private static bool Unique(IEnumerable<string> ids)
    {
        var list = ids.ToList();
        return list.Distinct(StringComparer.Ordinal).Count() == list.Count;
    }

    private static DomainResult Require(bool condition, string field, string message) =>
        condition ? DomainResult.Success() : EmbroiderySettingsErrors.Invalid(field, message);
}
