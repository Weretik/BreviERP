using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Shared;

namespace Crm.Application.Features.Embroidery.Shared.Mapping;

public static class EmbroideryCalculationMapper
{
    /// <summary>Call only after <c>CalculateEmbroideryInputValidator</c>: the codes are known names.</summary>
    public static EmbroideryInput ToDomain(this CalculateEmbroideryInput input) => new(
        Enum.Parse<EmbroideryMode>(input.Mode), input.Quantity, input.StitchesPerUnit, input.HoopId, input.UnitsPerHoop,
        input.StabilizerLayers, input.FabricLayers, Enum.Parse<EmbroideryProgram>(input.Program), input.ProgramMinutes,
        input.MachineMinutes, Enum.Parse<EmbroideryMaterialMode>(input.MaterialMode),
        input.ExpenseOverrides ?? new Dictionary<string, bool>());

    public static EmbroideryCalculationResponse Failed(string error) =>
        new(error, [], 0, 0, 0, 0, [], null, null, null);

    /// <summary>A Manager gets the prices and the layout without cost, rates or markups (R-02).</summary>
    public static EmbroideryCalculationResponse ToResponse(this EmbroideryResult result, bool seesCost) => new(
        null,
        result.Missing,
        result.Hoopings,
        result.FullHoopings,
        result.LastHoopUnits,
        result.FreeInLastHoop,
        [.. result.Prices.Select(x => new EmbroideryPriceResponse(x.Name, seesCost ? x.MarkupPercent : null, x.Unit, x.Total,
            x.RepeatUnit))],
        seesCost
            ? new EmbroideryCostResponse(result.Cost, result.UnitCost, [.. result.Rows.Select(x => new EmbroideryCostRowResponse(
                x.Kind.ToString(), x.Name, x.Basis?.ToString(), x.Quantity, x.Rate, x.Amount, x.PerUnit))])
            : null,
        result.Stabilizer.ToResponse(seesCost),
        result.Fabric?.ToResponse(seesCost));

    public static EmbroideryFormResponse ToForm(this EmbroiderySettingsContent content, bool seesCost,
        DateTimeOffset? updatedAt) => new(
        seesCost,
        [.. content.Hoops.Select(x => new EmbroideryFormHoop(x.Id, x.Label, x.BlankWidthCm, x.BlankLengthCm))],
        [.. content.Expenses.Select(x => new EmbroideryFormExpense(x.Id, x.Name, x.Basis.ToString(), x.Mode.ToString(),
            x.Enabled, x.Rate is not null))],
        [.. content.Markups.Select(x => x.Name)],
        updatedAt);

    private static EmbroideryCutResponse ToResponse(this EmbroideryCut cut, bool seesCost) => new(
        cut.BlankWidthCm, cut.BlankLengthCm, cut.RollWidthCm, cut.Blanks, cut.Across, cut.Rows, cut.Produced, cut.Spare,
        cut.SideWasteCm, cut.CutMetres, seesCost ? cut.BlankCost : null, seesCost ? cut.WholeCost : null,
        seesCost ? cut.Charged : null);
}
