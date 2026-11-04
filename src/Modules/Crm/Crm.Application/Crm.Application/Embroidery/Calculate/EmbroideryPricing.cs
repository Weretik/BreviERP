using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Application.Embroidery.Calculate;

/// <summary>A calculation with the rates in force now, and those rates as a snapshot for saving.</summary>
internal sealed record EmbroideryPriced(EmbroideryCalculation Calculation, EmbroideryRateSnapshot? Snapshot);

internal static class EmbroideryPricing
{
    /// <summary>
    /// Prices <paramref name="input"/> with the saved «Налаштування» (or the defaults before the first save) and the
    /// stitch rate from «Параметри ціноутворення»; without a stitch rate there is no price (R-09).
    /// </summary>
    public static async Task<EmbroideryPriced> PriceAsync(IEmbroideryStore store, IEmbroideryStitchRateReader stitchRate,
        EmbroideryInput input, CancellationToken cancellationToken)
    {
        var rate = await stitchRate.ReadAsync(cancellationToken);
        if (rate.Value is not { } stitch || stitch < 0)
            return new EmbroideryPriced(EmbroideryCalculation.Failed(EmbroideryMessages.StitchRateMissing), null);

        var saved = await store.FindSettingsAsync(tracking: false, cancellationToken);
        var content = saved?.Content ?? EmbroiderySettingsDefaults.Content;
        return new EmbroideryPriced(EmbroideryCalculator.Calculate(input, content, stitch),
            new EmbroideryRateSnapshot(stitch, content));
    }
}
