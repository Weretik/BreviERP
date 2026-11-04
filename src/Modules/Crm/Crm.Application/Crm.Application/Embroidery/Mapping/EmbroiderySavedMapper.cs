using Crm.Application.Embroidery.Contracts;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;

namespace Crm.Application.Embroidery.Mapping;

public static class EmbroiderySavedMapper
{
    public static CalculateEmbroideryInput ToContract(this EmbroideryInput input) => new(
        input.Mode.ToString(), input.Quantity, input.StitchesPerUnit, input.HoopId, input.UnitsPerHoop,
        input.StabilizerLayers, input.FabricLayers, input.Program.ToString(), input.ProgramMinutes,
        input.MachineMinutes, input.MaterialMode.ToString(), input.ExpenseOverrides);

    public static EmbroiderySavedSummaryResponse ToSummary(this EmbroiderySavedCalculation saved) => new(
        saved.Id,
        EmbroiderySavedCalculation.NumberOf(saved.Id),
        saved.Name,
        saved.CustomerPublicId,
        saved.CustomerName,
        saved.Mode.ToString(),
        saved.Quantity,
        saved.Input.StitchesPerUnit,
        saved.Snapshot.Settings.Hoops.FirstOrDefault(x => x.Id == saved.Input.HoopId)?.Label ?? saved.Input.HoopId,
        saved.Incomplete,
        [.. saved.Result.Prices.Select(x => new EmbroiderySavedPriceResponse(x.Name, x.Unit, x.Total, x.RepeatUnit))],
        saved.CreatedAt,
        saved.CreatedByName);

    /// <summary>A Manager gets the saved prices and layout, never the cost or the frozen rates (R-02).</summary>
    public static EmbroiderySavedCalculationResponse ToResponse(this EmbroiderySavedCalculation saved, bool seesCost)
    {
        var settings = saved.Snapshot.Settings;
        return new EmbroiderySavedCalculationResponse(
            saved.ToSummary(),
            saved.Input.ToContract(),
            saved.Result.ToResponse(seesCost),
            seesCost
                ? new EmbroiderySnapshotRatesResponse(
                    saved.Snapshot.StitchRate,
                    new EmbroideryRatesDto(settings.Rates.ThreadRate, settings.Rates.HoopingRate,
                        settings.Rates.ProgramHourRate, settings.Rates.TaxPercent),
                    [.. settings.Markups.Select(x => new EmbroideryMarkupDto(x.Name, x.Percent))])
                : null);
    }
}
