using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Application.Embroidery.Mapping;

public static class EmbroiderySettingsMapper
{
    public static EmbroiderySettingsResponse ToResponse(this EmbroiderySettings? saved, EmbroideryStitchRate stitchRate)
    {
        var content = saved?.Content ?? EmbroiderySettingsDefaults.Content;
        return new EmbroiderySettingsResponse(
            saved?.Version ?? 0,
            saved?.UpdatedByName,
            saved?.UpdatedAt,
            new EmbroideryStitchRateResponse(stitchRate.Value, stitchRate.UpdatedOn),
            new EmbroideryRatesDto(content.Rates.ThreadRate, content.Rates.HoopingRate, content.Rates.ProgramHourRate,
                content.Rates.TaxPercent),
            [.. content.Markups.Select(x => new EmbroideryMarkupDto(x.Name, x.Percent))],
            new EmbroideryMaterialDto(content.Stabilizer.PricePerMetre, content.Stabilizer.WidthCm),
            new EmbroideryMaterialDto(content.Fabric.PricePerMetre, content.Fabric.WidthCm),
            content.FabricIncreasePercent,
            [.. content.Hoops.Select(x => new EmbroideryHoopDto(x.Id, x.Label, x.BlankWidthCm, x.BlankLengthCm, x.GlueRate))],
            [.. content.Expenses.Select(x => new EmbroideryExpenseDto(x.Id, x.Name, x.Basis.ToString(), x.Rate, x.Enabled,
                x.Mode.ToString()))]);
    }

    /// <summary>Call only after <c>SaveEmbroiderySettingsInputValidator</c>: names are trimmed, codes parsed.</summary>
    public static EmbroiderySettingsContent ToContent(this SaveEmbroiderySettingsInput input) => new(
        new EmbroideryRates(input.Rates.ThreadRate, input.Rates.HoopingRate, input.Rates.ProgramHourRate, input.Rates.TaxPercent),
        [.. input.Markups.Select(x => new EmbroideryMarkup(x.Name.Trim(), x.Percent))],
        new EmbroideryMaterial(input.Stabilizer.PricePerMetre, input.Stabilizer.WidthCm),
        new EmbroideryMaterial(input.Fabric.PricePerMetre, input.Fabric.WidthCm),
        input.FabricIncreasePercent,
        [.. input.Hoops.Select(x => new EmbroideryHoop(x.Id.Trim(), x.Label.Trim(), x.BlankWidthCm, x.BlankLengthCm, x.GlueRate))],
        [.. input.Expenses.Select(x => new EmbroideryExpense(x.Id.Trim(), x.Name.Trim(),
            Enum.Parse<EmbroideryExpenseBasis>(x.Basis), x.Rate, x.Enabled, Enum.Parse<EmbroideryExpenseMode>(x.Mode)))]);
}
