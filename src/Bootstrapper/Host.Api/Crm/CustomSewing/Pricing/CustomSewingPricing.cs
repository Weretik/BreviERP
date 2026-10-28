using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.CustomEvaluation;
using Crm.Application.CustomSewing.Pricing;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;
using Host.Api.Crm.CustomSewing.Mapping;

namespace Host.Api.Crm.CustomSewing.Pricing;

public sealed class CustomSewingPricing(ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : ICustomSewingPricing
{
    public async Task<CustomPriceBasis> CalculateAsync(CustomConfiguration configuration, DateTimeOffset at, CancellationToken ct)
    {
        var input = configuration.Freeze();
        var result = await new SewingCustomPricingEvaluator(settings, references)
            .EvaluateAsync(CustomPricingInputMapper.Map(input), at, ct);
        if (!result.IsSuccess) throw new InvalidOperationException("Validated custom composition could not be evaluated.");
        return CustomPriceBasisMapper.Map(result.Value, input, at);
    }
}
