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
            return Result.Invalid(new ValidationError("articleId", "Неправильний товар або варіант."));
        at = at.ToUniversalTime();
        var snapshot = await inputs.ReadAsync(productId, articleId, cancellationToken);
        if (snapshot is null)
            return Result.NotFound();
        var context = await SewingPricingContext.ReadAsync(settings, references,
            SewingPricingInputResolution.Keys(snapshot), at, cancellationToken);
        var candidates = context.Candidates;
        var values = context.Values;
        var blockers = new List<SewingPricingBlocker>();
        if (!snapshot.IsActive)
            blockers.Add(new("InactiveArticle", snapshot.SaleSku, "Model or Article is inactive."));
        blockers.AddRange(context.Blockers);
        if (blockers.Count != 0)
            return Result.Success(Preview(new(null, null, blockers)));
        var byKey = context.ByKey;
        var calculation = SewingArticleCalculator.Calculate(candidates[0].Settings,
            SewingPricingInputResolution.Materials(snapshot, byKey), SewingPricingInputResolution.Operations(snapshot, byKey));
        return Result.Success(Preview(calculation));

        SewingArticlePricePreview Preview(SewingArticleCalculation calculation) =>
            new(snapshot.ProductId, snapshot.ArticleId, snapshot.SaleSku, snapshot.CompositionVersion,
                at, candidates.FirstOrDefault()?.Id, values.Select(x => x.VersionId).Distinct().ToArray(), calculation,
                snapshot, candidates.FirstOrDefault(), values);
    }
}


