using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.StitchRate;
using Mediator;
using Reference.Application.Features.PricingParameter.GetList;

namespace Host.Api.Crm;

/// <summary>Reads «Оплата фактичної вишивки за 1000 стібків» from «Параметри ціноутворення» (Reference).</summary>
internal sealed class EmbroideryStitchRateReader(ISender sender) : IEmbroideryStitchRateReader
{
    public const string Code = "EMBROIDERY_RATE_1000_STITCHES";

    public async Task<EmbroideryStitchRate> ReadAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPricingParametersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Pricing parameters could not be read.");
        var parameter = result.Value.FirstOrDefault(x => string.Equals(x.Code, Code, StringComparison.OrdinalIgnoreCase));
        return new EmbroideryStitchRate(parameter?.Value, parameter?.UpdatedOn);
    }
}
