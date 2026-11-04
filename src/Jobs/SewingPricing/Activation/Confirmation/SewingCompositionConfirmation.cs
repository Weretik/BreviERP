using Catalog.Application.Features.Product.Sewing;

namespace SewingPricing.Jobs.Activation;

/// <summary>Checks that the model composition is still the one the approved plan was built from.</summary>
public sealed class SewingCompositionConfirmation(ISewingCompositionReadStore reads)
{
    public async Task<long> ValidateAsync(ActivationModelPlan plan, CancellationToken cancellationToken)
    {
        var common = await reads.ReadCompositionAsync(plan.ProductId, null, cancellationToken)
            ?? throw new InvalidOperationException("Planned model is missing.");
        if (common.ModelCode != plan.ModelCode
            || !ActivationCompositionInputs.MaterialsEqual(plan.Materials, common.Materials.Select(ActivationCompositionInputs.Material))
            || !ActivationCompositionInputs.OperationsEqual(plan.Operations, common.Operations.Select(ActivationCompositionInputs.Operation)))
            throw new InvalidOperationException("Composition values changed after the approved plan.");
        foreach (var article in plan.Articles)
        {
            var current = await reads.ReadCompositionAsync(plan.ProductId, article.ArticleId, cancellationToken)
                ?? throw new InvalidOperationException("Planned Article is missing.");
            if (!current.ArticleIsActive || !ActivationCompositionInputs.MaterialsEqual(article.Materials,
                current.Materials.Where(x => x.ArticleId == article.ArticleId).Select(ActivationCompositionInputs.Material)))
                throw new InvalidOperationException("Article scope, activity or values changed after the approved plan.");
        }
        if (common.Version != plan.ExpectedVersion)
            throw new InvalidOperationException("Composition version changed after the approved plan; create a new plan.");
        return common.Version;
    }

    /// <summary>Nothing is written: the composition needs no confirmation, only the same values as planned.</summary>
    public Task<long> ConfirmAsync(ActivationModelPlan plan, CancellationToken cancellationToken)
        => ValidateAsync(plan, cancellationToken);
}
