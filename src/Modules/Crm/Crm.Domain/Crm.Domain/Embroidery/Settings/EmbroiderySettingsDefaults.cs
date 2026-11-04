namespace Crm.Domain.Embroidery.Settings;

/// <summary>
/// What the page shows before the first save: the rates the old calculator used on 2026-11-04 (its «Налаштування»
/// screen). The Director checks them and saves once; after that only the saved record is used.
/// </summary>
public static class EmbroiderySettingsDefaults
{
    public static EmbroiderySettingsContent Content { get; } = new(
        new EmbroideryRates(ThreadRate: 0.30m, HoopingRate: 10m, ProgramHourRate: 300m, TaxPercent: 24m),
        [new EmbroideryMarkup("Опт", 60m), new EmbroideryMarkup("Стандарт", 90m), new EmbroideryMarkup("Терміново", 120m)],
        new EmbroideryMaterial(PricePerMetre: 20m, WidthCm: 90m),
        new EmbroideryMaterial(PricePerMetre: 80m, WidthCm: 150m),
        FabricIncreasePercent: 40m,
        [
            new EmbroideryHoop("15", "15 см", 18m, 18m, null),
            new EmbroideryHoop("18", "18 см", 22m, 22m, null),
            new EmbroideryHoop("32", "32 см", 40m, 40m, null),
        ],
        [
            new EmbroideryExpense(EmbroideryExpense.GlueId, "Клей", EmbroideryExpenseBasis.Hoop, 4m, true, EmbroideryExpenseMode.All),
            new EmbroideryExpense("hoopSupplies", "Інші матеріали для зап’ялювання", EmbroideryExpenseBasis.Hoop, 1m, true, EmbroideryExpenseMode.All),
            new EmbroideryExpense("cutting", "Вирізання шеврона", EmbroideryExpenseBasis.Unit, 5m, true, EmbroideryExpenseMode.ChevronOnly),
            new EmbroideryExpense("edge", "Обробка краю", EmbroideryExpenseBasis.Unit, 2m, true, EmbroideryExpenseMode.ChevronOnly),
            new EmbroideryExpense("fastening", "Липучка або кріплення", EmbroideryExpenseBasis.Unit, 10m, true, EmbroideryExpenseMode.ChevronOnly),
            new EmbroideryExpense("unitOther", "Інші витрати на виріб", EmbroideryExpenseBasis.Hoop, 0m, true, EmbroideryExpenseMode.All),
            new EmbroideryExpense("machine", "Експлуатація машини", EmbroideryExpenseBasis.Stitches, 0.5m, true, EmbroideryExpenseMode.All),
            new EmbroideryExpense("preparation", "Підготовка партії та пробник", EmbroideryExpenseBasis.Order, 30m, true, EmbroideryExpenseMode.All),
            new EmbroideryExpense("orderOther", "Інші разові витрати", EmbroideryExpenseBasis.Order, 20m, true, EmbroideryExpenseMode.All),
        ]);
}
