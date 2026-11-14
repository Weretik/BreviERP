using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Settings;

namespace Catalog.Application.Features.Pricing.Shared.Sewing.Evaluation;

/// <summary>
/// The effective settings and reference versions of a calculation, and why they cannot be used. Shared by a Catalog
/// article and a free composition.
/// </summary>
internal sealed record SewingPricingContext(
    IReadOnlyList<SewingPricingSettingsSelection> Candidates,
    IReadOnlyList<PricingReferenceValue> Values,
    IReadOnlyList<SewingPricingBlocker> Blockers)
{
    public IReadOnlyDictionary<string, PricingReferenceValue> ByKey =>
        Values.GroupBy(x => x.ReferenceKey).ToDictionary(x => x.Key, x => x.First());

    public static async Task<SewingPricingContext> ReadAsync(
        ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references,
        IReadOnlyCollection<string> keys, DateTimeOffset at, CancellationToken cancellationToken)
    {
        var candidates = await settings.ReadCandidatesAsync(at, cancellationToken);
        var blockers = new List<SewingPricingBlocker>();
        if (candidates.Count == 0)
            blockers.Add(new("MissingSettings", null, "No approved effective sewing settings exist."));
        else if (candidates.Count > 1 && candidates[0].EffectiveFrom == candidates[1].EffectiveFrom)
            blockers.Add(new("AmbiguousSettings", null, "Multiple settings versions have the same effective instant."));
        if (candidates.Count > 0 && candidates[0].FormulaVersion != SewingArticleCalculation.FormulaVersion)
            blockers.Add(new("UnsupportedFormula", null, "Settings require a different formula implementation."));
        var values = await references.ReadAsync(keys, at, cancellationToken);
        foreach (var group in values.GroupBy(x => x.ReferenceKey).Where(x => x.Count() > 1))
            blockers.Add(new("AmbiguousReferenceVersion", group.Key, "Multiple effective cost/norm versions exist."));
        return new SewingPricingContext(candidates, values, blockers);
    }
}
