using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Application.Features.Product.Sewing;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Pricing.Sewing;
using Microsoft.Extensions.DependencyInjection;
using SewingPricing.Jobs.Readiness;

namespace SewingPricing.Jobs.Activation;

public static class PricingActivationPlanner
{
    public static async Task<PricingActivationPlan> BuildAsync(IServiceProvider services, string connectionString,
        string actor, IReadOnlyCollection<string> exclusions, CancellationToken cancellationToken = default)
    {
        var database = ActivationTargetGuard.Validate(connectionString);
        var inventory = await PricingReadinessJob.RunAsync(connectionString, DateTimeOffset.UtcNow, cancellationToken);
        if (inventory.Items.Any(x => x.SourceIssues.Count > 0 && !exclusions.Contains(x.SaleSku)))
            throw new InvalidOperationException("Unresolved source issue outside the confirmed exclusions.");
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var reads = provider.GetRequiredService<ISewingCompositionReadStore>();
        var inputs = provider.GetRequiredService<ISewingPricingInputReader>();
        var settings = provider.GetRequiredService<ISewingPricingSettingsReader>();
        var references = provider.GetRequiredService<IPricingReferenceVersionReader>();
        var evaluator = new CalculateSewingArticleQueryHandler(inputs, settings, references);
        var models = new List<ActivationModelPlan>();
        foreach (var group in inventory.Items.Where(x => x.Kind == "Sewing" && !exclusions.Contains(x.SaleSku)).GroupBy(x => x.ProductId))
        {
            var productId = group.Key ?? throw new InvalidOperationException("Missing model identity.");
            var common = await reads.ReadCompositionAsync(productId, null, cancellationToken) ?? throw new InvalidOperationException("Missing model.");
            var articles = new List<ActivationArticlePlan>();
            foreach (var item in group)
            {
                var id = item.ArticleId ?? throw new InvalidOperationException("Missing Article identity.");
                var current = await reads.ReadCompositionAsync(productId, id, cancellationToken) ?? throw new InvalidOperationException("Missing Article.");
                var preview = await evaluator.Handle(new(productId, id, DateTimeOffset.UtcNow), cancellationToken);
                if (!preview.IsSuccess || preview.Value.Settings is null || !preview.Value.Inputs.IsActive)
                    throw new InvalidOperationException("Planned Article/settings are not available and active.");
                var costs = preview.Value.References.ToDictionary(x => x.ReferenceKey);
                var diagnostic = SewingArticleCalculator.Calculate(preview.Value.Settings.Settings,
                    SewingPricingInputResolution.Materials(preview.Value.Inputs, costs).Select(x => x with { IsReviewed = true }).ToArray(),
                    SewingPricingInputResolution.Operations(preview.Value.Inputs, costs).Select(x => x with { IsReviewed = true }).ToArray());
                if (!diagnostic.IsCalculated) throw new InvalidOperationException("Planned Article has non-review pricing blockers.");
                articles.Add(new(id, item.SaleSku, current.Materials.Where(x => x.ArticleId == id).Select(ActivationCompositionInputs.Material).ToArray(),
                    preview.Value.SettingsVersionId, preview.Value.ReferenceVersionIds, diagnostic.Prices!, Guid.NewGuid()));
            }
            models.Add(new(productId, common.ModelCode, common.Version, common.Materials.Select(ActivationCompositionInputs.Material).ToArray(),
                common.Operations.Select(ActivationCompositionInputs.Operation).ToArray(), articles));
        }
        var ppe = provider.GetRequiredService<IPpePricingInputReader>();
        var offers = new List<ActivationOfferPlan>();
        var rules = new Dictionary<string, ActivationRulePlan>(StringComparer.Ordinal);
        foreach (var chunk in inventory.Items.Where(x => x.Kind == "Ppe" && !exclusions.Contains(x.SaleSku)).Chunk(100))
        {
            var values = await ppe.ReadAsync(chunk.Select(x => x.ProductId ?? throw new InvalidOperationException("Missing PPE identity.")).ToArray(), cancellationToken);
            foreach (var item in chunk)
            {
                var value = values.Single(x => x.ProductId == item.ProductId);
                if (!value.IsActive || value.Offer is not { } offer) throw new InvalidOperationException("PPE is inactive or lacks an offer.");
                var prices = PpeCalculator.Calculate(new(offer.Values.SupplierCost, true, offer.Values.ExternalRetail,
                    offer.Values.ExternalWholesale, true, value.Rule is { } rule ? [rule.Values.Markups] : []));
                if (prices.Prices is null) throw new InvalidOperationException("Planned PPE has non-review pricing blockers.");
                offers.Add(new(value.ProductId, item.SaleSku, offer.Id, Guid.NewGuid(), offer.Values, prices.Prices));
                if (value.Rule is { } selected) rules.TryAdd(selected.Values.Code, new(selected.Id, Guid.NewGuid(), selected.Values));
            }
        }
        return new(Guid.NewGuid(), database, actor, DateTimeOffset.UtcNow, inventory.SourceHash, models,
            rules.Values.ToArray(), offers, exclusions.ToArray());
    }
}
