using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Application.Features.Pricing.Sewing.AutoPublication.Specifications;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Pricing.Sewing.AutoPublication;

/// <summary>
/// Automatic Sewing price publication (CAT-012): saves and publishes a new calculation for every active Article whose
/// live price is calculable and was never published or differs from the published one. Runs after every saved change
/// of a pricing input and nightly in brevierp-sync. An Article that cannot be calculated keeps its published price.
/// </summary>
public sealed class SewingPricePublisher(
    ICatalogReadRepository<ProductEntity> products,
    SewingPriceStatusReader status,
    ISewingPricingTransaction transactions,
    ISewingCalculationStore calculations,
    ISewingPublicationStore publications,
    ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings,
    IPricingReferenceVersionReader references,
    TimeProvider timeProvider)
{
    /// <param name="productIds">The Sewing models to check; null checks every model. Ids of other products are ignored.</param>
    public async Task<SewingPublicationReport> PublishChangedAsync(
        IReadOnlyCollection<int>? productIds, string actor, CancellationToken cancellationToken)
    {
        var sewingIds = await products.ListAsync(new ProductIdsByTypeSpec(ProductType.Sewing), cancellationToken);
        var ids = productIds is null ? sewingIds : sewingIds.Intersect(productIds).ToList();

        var at = timeProvider.GetUtcNow();
        var models = await status.ReadAsync(ids, at, cancellationToken);
        var changes = new List<SewingPublishedChange>();
        var blocked = new List<SewingPublicationBlocked>();
        var unchanged = 0;
        foreach (var model in models)
        foreach (var article in model.Articles.Where(x => x.Active))
        {
            if (article.NeedsPublication)
                changes.Add(new(model.ProductId, model.ModelCode, article.ArticleId, article.SaleSku,
                    article.Published?.Prices, article.Live.Prices!));
            else if (!article.Live.IsCalculated)
                blocked.Add(new(model.ProductId, model.ModelCode, article.ArticleId, article.SaleSku,
                    article.Live.Blockers.Select(x => x.Code).Distinct().ToList()));
            else
                unchanged++;
        }

        var evaluator = new SewingArticlePricingEvaluator(inputs, settings, references);
        var published = new List<SewingPublishedChange>(changes.Count);
        var skipped = new List<string>();
        foreach (var product in changes.GroupBy(x => x.ProductId))
        {
            // One transaction per model, like a single «Опублікувати»: a model is published whole or not at all.
            await using var transaction = await transactions.BeginAsync(product.Key, cancellationToken);
            var saved = new List<SewingPublishedChange>();
            foreach (var change in product)
            {
                var preview = await evaluator.EvaluateAsync(change.ProductId, change.ArticleId, at, cancellationToken);
                if (!preview.IsSuccess || !preview.Value.Calculation.IsCalculated)
                    break;
                var calculation = new SavedSewingCalculation(Guid.NewGuid(), Guid.NewGuid(), at, actor, preview.Value);
                await calculations.SaveAsync(calculation, cancellationToken);
                await publications.PublishAsync(new PublishedSewingPrice(Guid.NewGuid(), calculation.Id, change.ProductId,
                    change.ArticleId, at, actor, preview.Value.Calculation.Prices!), cancellationToken);
                saved.Add(change with { After = preview.Value.Calculation.Prices! });
            }

            if (saved.Count != product.Count())
            {
                // Inputs changed while publishing: this model is skipped, the next change or the nightly run retries.
                skipped.Add(product.First().ModelCode ?? product.Key.ToString());
                continue;
            }

            await transactions.CommitAsync(cancellationToken);
            published.AddRange(saved);
        }

        return new SewingPublicationReport(unchanged, published, blocked, skipped);
    }
}

public sealed record SewingPublicationReport(
    int Unchanged,
    IReadOnlyList<SewingPublishedChange> Published,
    IReadOnlyList<SewingPublicationBlocked> Blocked,
    IReadOnlyList<string> Skipped);

/// <summary>One Article whose price was published; <paramref name="Before"/> is null when it had none.</summary>
public sealed record SewingPublishedChange(
    int ProductId, string? ModelCode, long ArticleId, string SaleSku,
    SewingPriceTiers? Before, SewingPriceTiers After);

/// <summary>An active Article whose price cannot be calculated; its published price, if any, stays.</summary>
public sealed record SewingPublicationBlocked(
    int ProductId, string? ModelCode, long ArticleId, string SaleSku, IReadOnlyList<string> Blockers);
