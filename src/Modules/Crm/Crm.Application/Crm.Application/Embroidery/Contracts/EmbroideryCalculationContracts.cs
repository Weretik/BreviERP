namespace Crm.Application.Embroidery.Contracts;

/// <summary>What the «Розрахунок» form needs; rates are left out for a Manager.</summary>
/// <param name="SeesCost">True for Director and Admin: the page shows cost, rates and «Налаштування».</param>
public sealed record EmbroideryFormResponse(
    bool SeesCost,
    IReadOnlyList<EmbroideryFormHoop> Hoops,
    IReadOnlyList<EmbroideryFormExpense> Expenses,
    IReadOnlyList<string> MarkupNames,
    DateTimeOffset? SettingsUpdatedAt);

public sealed record EmbroideryFormHoop(string Id, string Label, decimal BlankWidthCm, decimal BlankLengthCm);

/// <param name="HasRate">False: ticking it makes the price «Орієнтовна» until the rate is set.</param>
public sealed record EmbroideryFormExpense(string Id, string Name, string Basis, string Mode, bool Enabled, bool HasRate);

/// <param name="Mode">`Garment` («Вишивка на виробі») or `Chevron` («Шеврони»).</param>
/// <param name="Program">`New`, `Adjustment` or `Ready`.</param>
/// <param name="MaterialMode">`UsedBlanks` or `FullCut`.</param>
/// <param name="ExpenseOverrides">Extra costs ticked or unticked for this calculation only, by id.</param>
public sealed record CalculateEmbroideryInput(
    string Mode,
    int Quantity,
    int StitchesPerUnit,
    string HoopId,
    int UnitsPerHoop,
    int StabilizerLayers,
    int FabricLayers,
    string Program,
    int ProgramMinutes,
    decimal? MachineMinutes,
    string MaterialMode,
    IReadOnlyDictionary<string, bool>? ExpenseOverrides);

/// <summary>
/// The live price. <paramref name="Error"/> stops the price (no prices then). <paramref name="Missing"/> names
/// ticked costs without a rate: the prices are «Орієнтовна». <paramref name="Cost"/> is null for a Manager.
/// </summary>
public sealed record EmbroideryCalculationResponse(
    string? Error,
    IReadOnlyList<string> Missing,
    int Hoopings,
    int FullHoopings,
    int LastHoopUnits,
    int FreeInLastHoop,
    IReadOnlyList<EmbroideryPriceResponse> Prices,
    EmbroideryCostResponse? Cost,
    EmbroideryCutResponse? Stabilizer,
    EmbroideryCutResponse? Fabric);

/// <param name="MarkupPercent">Null for a Manager.</param>
public sealed record EmbroideryPriceResponse(string Name, decimal? MarkupPercent, decimal Unit, decimal Total, decimal RepeatUnit);

public sealed record EmbroideryCostResponse(decimal Total, decimal PerUnit, IReadOnlyList<EmbroideryCostRowResponse> Rows);

/// <param name="Kind">`Stitches`, `Threads`, `Hooping`, `Stabilizer`, `Fabric`, `Program` or `Extra`.</param>
/// <param name="Quantity">Thousands of stitches, hoopings, blanks, minutes of development, units, machine hours or 1.</param>
public sealed record EmbroideryCostRowResponse(
    string Kind,
    string Name,
    string? Basis,
    decimal Quantity,
    decimal? Rate,
    decimal Amount,
    decimal PerUnit);

/// <summary>How a material is cut from its roll; money fields are null for a Manager.</summary>
public sealed record EmbroideryCutResponse(
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
    decimal? BlankCost,
    decimal? WholeCost,
    decimal? Charged);
