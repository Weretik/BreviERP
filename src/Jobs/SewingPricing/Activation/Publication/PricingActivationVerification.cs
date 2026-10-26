using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Microsoft.Extensions.DependencyInjection;

namespace SewingPricing.Jobs.Activation;

public static class PricingActivationVerification
{
    public static async Task<ActivationVerificationResult> VerifyAsync(IServiceProvider services, PricingActivationPlan plan,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var published = provider.GetRequiredService<ISewingPublishedPriceReader>();
        var calculations = provider.GetRequiredService<ISewingCalculationStore>();
        var sewingCount = 0;
        foreach (var chunk in plan.Sewing.Chunk(100))
        {
            var current = await published.ReadAsync(chunk.Select(x => x.ProductId).ToArray(), cancellationToken);
            foreach (var model in chunk)
            {
                var product = current.Single(x => x.ProductId == model.ProductId);
                foreach (var article in model.Articles)
                {
                    var price = product.Articles.Single(x => x.ArticleId == article.ArticleId);
                    var saved = await calculations.FindRequestAsync(article.SaveRequestId, cancellationToken);
                    if (price.Prices != article.ExpectedPrices || saved is null || price.CalculationId != saved.Id
                        || saved.Snapshot.ProductId != model.ProductId || saved.Snapshot.ArticleId != article.ArticleId
                        || !saved.Snapshot.Calculation.IsCalculated)
                        throw new InvalidOperationException("Reopened publication/calculation does not match the activation plan.");
                    sewingCount++;
                }
            }
        }
        var ppe = provider.GetRequiredService<IPpePricingInputReader>();
        var ppeCount = 0;
        foreach (var chunk in plan.Offers.Chunk(100))
        {
            var current = await ppe.ReadAsync(chunk.Select(x => x.ProductId).ToArray(), cancellationToken);
            foreach (var offer in chunk)
            {
                var input = current.Single(x => x.ProductId == offer.ProductId);
                if (input.Offer is not { IsApproved: true } || !input.IsActive
                    || PpeProductPricingEvaluator.Evaluate(input).Calculation.Prices != offer.ExpectedPrices)
                    throw new InvalidOperationException("Reopened PPE input/result does not match the activation plan.");
                ppeCount++;
            }
        }
        return new(plan.Id, sewingCount, ppeCount);
    }
}
