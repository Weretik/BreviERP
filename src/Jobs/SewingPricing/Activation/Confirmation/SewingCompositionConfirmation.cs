using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.Sewing;

namespace SewingPricing.Jobs.Activation;

public sealed class SewingCompositionConfirmation(ISewingCompositionStore store, ISewingCompositionReadStore reads,
    IProductReferenceReader references)
{
    public async Task<(long Version, bool CommonReviewed, IReadOnlySet<long> ReviewedArticles)> ValidateAsync(
        ActivationModelPlan plan, CancellationToken cancellationToken)
    {
        var common = await reads.ReadCompositionAsync(plan.ProductId, null, cancellationToken)
            ?? throw new InvalidOperationException("Planned model is missing.");
        if (common.ModelCode != plan.ModelCode
            || !ActivationCompositionInputs.MaterialsEqual(plan.Materials, common.Materials.Select(ActivationCompositionInputs.Material))
            || !ActivationCompositionInputs.OperationsEqual(plan.Operations, common.Operations.Select(ActivationCompositionInputs.Operation)))
            throw new InvalidOperationException("Composition values changed after the approved plan.");
        var commonReviewed = common.Materials.All(x => x.Reviewed) && common.Operations.All(x => x.Reviewed);
        // The existing common handler calls ReplaceComposition and SetModelCode; each increments the version.
        var completed = commonReviewed && !(plan.Materials.All(x => x.Reviewed) && plan.Operations.All(x => x.Reviewed)) ? 2 : 0;
        var reviewed = new HashSet<long>();
        foreach (var article in plan.Articles)
        {
            var current = await reads.ReadCompositionAsync(plan.ProductId, article.ArticleId, cancellationToken)
                ?? throw new InvalidOperationException("Planned Article is missing.");
            if (!current.ArticleIsActive || !ActivationCompositionInputs.MaterialsEqual(article.Materials,
                current.Materials.Where(x => x.ArticleId == article.ArticleId).Select(ActivationCompositionInputs.Material)))
                throw new InvalidOperationException("Article scope, activity or values changed after the approved plan.");
            if (current.Materials.Where(x => x.ArticleId == article.ArticleId).All(x => x.Reviewed))
            {
                reviewed.Add(article.ArticleId);
                if (!article.Materials.All(x => x.Reviewed)) completed++;
            }
        }
        if (common.Version != plan.ExpectedVersion + completed)
            throw new InvalidOperationException("Unrecognized composition version change; create a reviewed next plan.");
        return (common.Version, commonReviewed, reviewed);
    }

    public async Task<long> ConfirmAsync(ActivationModelPlan plan, string actor, CancellationToken cancellationToken)
    {
        var state = await ValidateAsync(plan, cancellationToken);
        var version = state.Version;
        if (!state.CommonReviewed)
        {
            var request = new ReplaceSewingCompositionRequest(plan.ModelCode,
                plan.Materials.Select(x => x with { Reviewed = true }).ToArray(),
                plan.Operations.Select(x => x with { Reviewed = true }).ToArray(), version);
            var result = await new ReplaceSewingCompositionHandler(store, reads, references)
                .Handle(new(plan.ProductId, request, actor), cancellationToken);
            if (!result.IsSuccess) throw new InvalidOperationException($"Common composition confirmation: {result.Status}.");
            version = result.Value.Version;
        }
        foreach (var article in plan.Articles.Where(x => !state.ReviewedArticles.Contains(x.ArticleId)))
        {
            var request = new ReplaceSewingArticleCompositionRequest(
                article.Materials.Select(x => x with { Reviewed = true }).ToArray(), version);
            var result = await new ReplaceSewingArticleCompositionHandler(store, reads, references)
                .Handle(new(plan.ProductId, article.ArticleId, request, actor), cancellationToken);
            if (!result.IsSuccess) throw new InvalidOperationException($"Article composition confirmation: {result.Status}.");
            version = result.Value.Version;
        }
        return version;
    }
}
