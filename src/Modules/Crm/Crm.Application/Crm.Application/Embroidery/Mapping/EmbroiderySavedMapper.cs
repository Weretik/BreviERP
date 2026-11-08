using Crm.Application.Embroidery.Contracts;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Application.Embroidery.Mapping;

public static class EmbroiderySavedMapper
{
    public static CalculateEmbroideryInput ToContract(this EmbroideryInput input) => new(
        input.Mode.ToString(), input.Quantity, input.StitchesPerUnit, input.HoopId, input.UnitsPerHoop,
        input.StabilizerLayers, input.FabricLayers, input.Program.ToString(), input.ProgramMinutes,
        input.MachineMinutes, input.MaterialMode.ToString(), input.ExpenseOverrides);

    public static EmbroiderySavedSummaryResponse ToSummary(this EmbroiderySavedCalculation saved,
        EmbroideryMockupResponse? mockup = null) => new(
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
        saved.Price is { } price
            ? new EmbroiderySavedPriceResponse(EmbroideryCategories.Label(price.Name), price.Unit, price.Total,
                price.RepeatUnit)
            : null,
        saved.CreatedAt,
        saved.CreatedByName,
        mockup);

    /// <summary>The saved result with only the chosen price, under its category name.</summary>
    private static EmbroideryResult Chosen(EmbroiderySavedCalculation saved) => saved.Result with
    {
        Prices = saved.Price is { } price ? [price with { Name = EmbroideryCategories.Label(price.Name) }] : [],
    };

    /// <summary>A Manager gets the saved price and layout, never the cost or the frozen rates (R-02).</summary>
    public static EmbroiderySavedCalculationResponse ToResponse(this EmbroiderySavedCalculation saved, bool seesCost,
        EmbroideryMockupResponse? mockup = null)
    {
        var settings = saved.Snapshot.Settings;
        return new EmbroiderySavedCalculationResponse(
            saved.ToSummary(mockup),
            saved.Input.ToContract(),
            Chosen(saved).ToResponse(seesCost),
            seesCost
                ? new EmbroiderySnapshotRatesResponse(
                    saved.Snapshot.StitchRate,
                    new EmbroideryRatesDto(settings.Rates.ThreadRate, settings.Rates.HoopingRate,
                        settings.Rates.ProgramHourRate, settings.Rates.TaxPercent),
                    [.. settings.Markups.Select(x => new EmbroideryMarkupDto(x.Name, x.Percent))])
                : null);
    }
}
