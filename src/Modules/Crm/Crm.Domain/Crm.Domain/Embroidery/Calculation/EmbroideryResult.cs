using Crm.Domain.Embroidery.Settings;

namespace Crm.Domain.Embroidery.Calculation;

/// <summary>Either a price or the first problem that stops it (for example a blank wider than the roll).</summary>
public sealed record EmbroideryCalculation(string? Error, EmbroideryResult? Result)
{
    public static EmbroideryCalculation Failed(string error) => new(error, null);
}

/// <summary>
/// A computed price. Money is rounded half-up to kopecks only here, from exact values (R-07).
/// <see cref="Missing"/> lists costs that were ticked but have no rate: the price is then «Орієнтовна».
/// </summary>
public sealed record EmbroideryResult(
    decimal Cost,
    decimal UnitCost,
    int Hoopings,
    int FullHoopings,
    int LastHoopUnits,
    int FreeInLastHoop,
    EmbroideryCut Stabilizer,
    EmbroideryCut? Fabric,
    IReadOnlyList<EmbroideryCostRow> Rows,
    IReadOnlyList<EmbroideryPrice> Prices,
    IReadOnlyList<string> Missing)
{
    public bool Incomplete => Missing.Count > 0;
}

/// <param name="Quantity">
/// What the rate is multiplied by: thousands of stitches, hoopings, blanks, minutes of development, units, machine
/// hours or 1 order.
/// </param>
/// <param name="Rate">The rate actually used (for glue, the hoop's own rate when it has one).</param>
public sealed record EmbroideryCostRow(
    EmbroideryCostKind Kind,
    string Name,
    EmbroideryExpenseBasis? Basis,
    decimal Quantity,
    decimal? Rate,
    decimal Amount,
    decimal PerUnit);

public enum EmbroideryCostKind
{
    Stitches = 0,
    Threads = 1,
    Hooping = 2,
    Stabilizer = 3,
    Fabric = 4,
    Program = 5,
    Extra = 6,
}

/// <summary>How one material is cut from its roll. <see cref="Charged"/> is what the cost includes.</summary>
public sealed record EmbroideryCut(
    decimal BlankWidthCm,
    decimal BlankLengthCm,
    decimal RollWidthCm,
    int Blanks,
    int Across,
    int Rows,
    int Produced,
    int Spare,
    decimal SideWasteCm,
    decimal CutMetres,
    decimal BlankCost,
    decimal WholeCost,
    decimal Charged);

/// <param name="RepeatUnit">The same embroidery again with the ready program: only development is left out.</param>
public sealed record EmbroideryPrice(string Name, decimal MarkupPercent, decimal Unit, decimal Total, decimal RepeatUnit);
