using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Evaluation;

public sealed class SewingArticlePricingEvaluator(
    ISewingPricingInputReader inputs, ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
{
    public async ValueTask<Result<SewingArticlePricePreview>> EvaluateAsync(
        int productId, long articleId, DateTimeOffset at, CancellationToken cancellationToken)
    {
        if (productId <= 0 || articleId <= 0)
            return Result.Invalid(new ValidationError("articleId", "Positive Product and Article IDs are required."));
        at = at.ToUniversalTime();
        var snapshot = await inputs.ReadAsync(productId, articleId, cancellationToken);
        if (snapshot is null)
            return Result.NotFound();
        var candidates = await settings.ReadCandidatesAsync(at, cancellationToken);
        var blockers = new List<SewingPricingBlocker>();
        if (!snapshot.IsActive)
            blockers.Add(new("InactiveArticle", snapshot.SaleSku, "Model or Article is inactive."));
        if (candidates.Count == 0)
            blockers.Add(new("MissingSettings", null, "No approved effective sewing settings exist."));
        else if (candidates.Count > 1 && candidates[0].EffectiveFrom == candidates[1].EffectiveFrom)
            blockers.Add(new("AmbiguousSettings", null, "Multiple settings versions have the same effective instant."));
        if (candidates.Count > 0 && candidates[0].FormulaVersion != SewingArticleCalculation.FormulaVersion)
            blockers.Add(new("UnsupportedFormula", null, "Settings require a different formula implementation."));
        var values = await references.ReadAsync(SewingPricingInputResolution.Keys(snapshot), at, cancellationToken);
        foreach (var group in values.GroupBy(x => x.ReferenceKey).Where(x => x.Count() > 1))
            blockers.Add(new("AmbiguousReferenceVersion", group.Key, "Multiple effective cost/norm versions exist."));
        if (blockers.Count != 0)
            return Result.Success(Preview(new(null, null, blockers)));
        var byKey = values.ToDictionary(x => x.ReferenceKey);
        var calculation = SewingArticleCalculator.Calculate(candidates[0].Settings,
            SewingPricingInputResolution.Materials(snapshot, byKey), SewingPricingInputResolution.Operations(snapshot, byKey));
        return Result.Success(Preview(calculation));

        SewingArticlePricePreview Preview(SewingArticleCalculation calculation) =>
            new(snapshot.ProductId, snapshot.ArticleId, snapshot.SaleSku, snapshot.CompositionVersion,
                at, candidates.FirstOrDefault()?.Id, values.Select(x => x.VersionId).Distinct().ToArray(), calculation,
                snapshot, candidates.FirstOrDefault(), values);
    }
}


