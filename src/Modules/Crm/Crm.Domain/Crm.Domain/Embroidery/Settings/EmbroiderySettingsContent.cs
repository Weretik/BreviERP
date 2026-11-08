namespace Crm.Domain.Embroidery.Settings;

/// <summary>
/// Everything «Ціна вишивки › Налаштування» edits (EMB-001). The embroiderer's rate is not here: it is the pricing
/// parameter <c>EMBROIDERY_RATE_1000_STITCHES</c> of «Параметри ціноутворення», the single source.
/// </summary>
public sealed record EmbroiderySettingsContent(
    EmbroideryRates Rates,
    IReadOnlyList<EmbroideryMarkup> Markups,
    EmbroideryMaterial Stabilizer,
    EmbroideryMaterial Fabric,
    decimal FabricIncreasePercent,
    IReadOnlyList<EmbroideryHoop> Hoops,
    IReadOnlyList<EmbroideryExpense> Expenses);

/// <param name="ThreadRate">«Нитки», грн per 1000 stitches.</param>
/// <param name="HoopingRate">«Зап’ялювання», грн per hooping.</param>
/// <param name="ProgramHourRate">«Розробка програми», грн per hour.</param>
/// <param name="TaxPercent">«Податкове навантаження»: a multiplier after the markup, not VAT.</param>
public sealed record EmbroideryRates(decimal ThreadRate, decimal HoopingRate, decimal ProgramHourRate, decimal TaxPercent);

/// <summary>One of exactly two price categories («Категорія 1», «Категорія 2»).</summary>
public sealed record EmbroideryMarkup(string Name, decimal Percent);

/// <summary>A roll material: price per running metre and roll width in cm.</summary>
public sealed record EmbroideryMaterial(decimal PricePerMetre, decimal WidthCm);

/// <param name="Id">Stable id kept by saved calculations; the label is only a caption.</param>
/// <param name="GlueRate">Glue for this size; null means the general «Клей» rate. Zero overrides it.</param>
public sealed record EmbroideryHoop(string Id, string Label, decimal BlankWidthCm, decimal BlankLengthCm, decimal? GlueRate);

/// <param name="Rate">Null means «not set yet» (the price becomes incomplete); zero is an explicit free item.</param>
/// <param name="Enabled">Whether a new calculation starts with this item ticked.</param>
public sealed record EmbroideryExpense(
    string Id,
    string Name,
    EmbroideryExpenseBasis Basis,
    decimal? Rate,
    bool Enabled,
    EmbroideryExpenseMode Mode)
{
    /// <summary>The glue item may take its rate from the chosen hoop.</summary>
    public const string GlueId = "glue";
}

/// <summary>What one unit of an extra cost is counted per.</summary>
public enum EmbroideryExpenseBasis
{
    /// <summary>1000 stitches of the whole batch.</summary>
    Stitches = 0,

    /// <summary>One hooping.</summary>
    Hoop = 1,

    /// <summary>One finished unit.</summary>
    Unit = 2,

    /// <summary>One hour of machine time.</summary>
    MachineHour = 3,

    /// <summary>Once per order.</summary>
    Order = 4,
}

/// <summary>Where an extra cost applies.</summary>
public enum EmbroideryExpenseMode
{
    All = 0,
    ChevronOnly = 1,
}
