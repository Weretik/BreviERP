using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Settings.GetCurrent;

public sealed class GetSewingSettingsQueryHandler(ISewingPricingSettingsReader reader)
    : IQueryHandler<GetSewingSettingsQuery, Result<SewingPricingSettingsSelection>>
{
    public async ValueTask<Result<SewingPricingSettingsSelection>> Handle(
        GetSewingSettingsQuery query, CancellationToken cancellationToken)
    {
        var candidates = await reader.ReadCandidatesAsync(DateTimeOffset.UtcNow, cancellationToken);
        if (candidates.Count == 0)
            return Result.NotFound();
        if (candidates.Count > 1 && candidates[0].EffectiveFrom == candidates[1].EffectiveFrom)
            return Result.Conflict("Current settings are ambiguous.");
        return Result.Success(candidates[0]);
    }
}
