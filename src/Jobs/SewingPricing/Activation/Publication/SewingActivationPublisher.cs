using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;
using Catalog.Application.Features.Pricing.Commands.SaveSewingCalculation;
using Catalog.Application.Features.Pricing.Queries.CalculateSewingArticle;

namespace SewingPricing.Jobs.Activation;

public sealed class SewingActivationPublisher(ISewingPricingTransaction transactions, ISewingCalculationStore calculations,
    ISewingPublicationStore publications, ISewingPricingInputReader inputs, ISewingPricingSettingsReader settings,
    IPricingReferenceVersionReader references)
{
    public async Task ValidateInputsAsync(int productId, ActivationArticlePlan plan, CancellationToken cancellationToken)
    {
        var preview = await new CalculateSewingArticleHandler(inputs, settings, references)
            .Handle(new(productId, plan.ArticleId, DateTimeOffset.UtcNow), cancellationToken);
        if (!preview.IsSuccess || !preview.Value.Inputs.IsActive || preview.Value.SettingsVersionId != plan.SettingsVersionId
            || !preview.Value.ReferenceVersionIds.Order().SequenceEqual(plan.ReferenceVersionIds.Order()))
            throw new InvalidOperationException("Planned Article activity/settings/reference versions changed.");
    }

    public async Task<PricingActivationStep> PublishAsync(int productId, long compositionVersion, ActivationArticlePlan plan,
        string actor, CancellationToken cancellationToken)
    {
        var preview = await new CalculateSewingArticleHandler(inputs, settings, references)
            .Handle(new(productId, plan.ArticleId, DateTimeOffset.UtcNow), cancellationToken);
        if (!preview.IsSuccess || !preview.Value.Calculation.IsCalculated || preview.Value.CompositionVersion != compositionVersion
            || preview.Value.SettingsVersionId != plan.SettingsVersionId
            || !preview.Value.ReferenceVersionIds.Order().SequenceEqual(plan.ReferenceVersionIds.Order()))
            throw new InvalidOperationException("Current strict calculation is blocked or planned input versions changed.");
        var saved = await new SaveSewingCalculationHandler(transactions, calculations, inputs, settings, references)
            .Handle(new(productId, plan.ArticleId, plan.SaveRequestId, actor), cancellationToken);
        if (!saved.IsSuccess || saved.Value.Snapshot.CompositionVersion != compositionVersion
            || saved.Value.Snapshot.SettingsVersionId != plan.SettingsVersionId
            || !saved.Value.Snapshot.ReferenceVersionIds.Order().SequenceEqual(plan.ReferenceVersionIds.Order())
            || saved.Value.Snapshot.Calculation.Prices != plan.ExpectedPrices)
            throw new InvalidOperationException("Saved calculation does not match the approved planned inputs/result.");
        var published = await new PublishSewingCalculationHandler(transactions, calculations, publications, inputs, settings, references)
            .Handle(new(productId, plan.ArticleId, saved.Value.Id, actor), cancellationToken);
        if (!published.IsSuccess) throw new InvalidOperationException($"Publication rejected: {published.Status}.");
        return new(plan.SaleSku, "Published", null, published.Value.Id.ToString(), saved.Value.Id, published.Value.Id);
    }
}
