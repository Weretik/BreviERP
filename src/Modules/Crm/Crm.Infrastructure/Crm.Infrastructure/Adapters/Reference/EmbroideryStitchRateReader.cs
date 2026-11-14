
using Reference.Contracts.PricingParameters;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Infrastructure.Adapters.Reference;

/// <summary>Reads «Оплата фактичної вишивки за 1000 стібків» from «Параметри ціноутворення» (Reference).</summary>
internal sealed class EmbroideryStitchRateReader(IPricingParameterDirectory parameters) : IEmbroideryStitchRateReader
{
    public const string Code = "EMBROIDERY_RATE_1000_STITCHES";

    public async Task<EmbroideryStitchRate> ReadAsync(CancellationToken cancellationToken)
    {
        var parameter = (await parameters.ListAsync(cancellationToken))
            .FirstOrDefault(x => string.Equals(x.Code, Code, StringComparison.OrdinalIgnoreCase));
        return new EmbroideryStitchRate(parameter?.Value, parameter?.UpdatedOn);
    }
}
