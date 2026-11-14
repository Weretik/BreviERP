using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared.Sewing.Evaluation;
using Catalog.Application.Abstractions.Pricing.Inputs;
using Catalog.Application.Abstractions.Pricing.Publications;
using Catalog.Application.Abstractions.Pricing.Settings;
using Catalog.Application.Abstractions.Sewing;

namespace Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

/// <summary>
/// The published and the live price of every Article of the given Sewing models (CAT-011). A live price that differs
/// from the published one is published automatically by <see cref="SewingPricePublisher"/>.
/// </summary>
public sealed class SewingPriceStatusReader(
    ISewingCompositionReadStore composition,
    ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings,
    IPricingReferenceVersionReader references,
    ISewingPublishedPriceReader published)
{
    private const string FabricKind = "Fabric";

    public async Task<IReadOnlyList<SewingModelPriceStatus>> ReadAsync(
        IReadOnlyCollection<int> productIds, DateTimeOffset at, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
            return [];
        var publishedByArticle = (await published.ReadAsync(productIds, cancellationToken))
            .SelectMany(product => product.Articles).ToDictionary(article => article.ArticleId);
        var evaluator = new SewingArticlePricingEvaluator(inputs, settings, references);
        var models = new List<SewingModelPriceStatus>(productIds.Count);
        foreach (var productId in productIds)
        {
            var list = await composition.ReadArticlesAsync(productId, cancellationToken);
            if (list is null)
                continue;
            var articles = new List<SewingArticlePriceStatus>(list.Articles.Count);
            foreach (var article in list.Articles.OrderBy(x => x.SortOrder).ThenBy(x => x.Id))
            {
                var preview = await evaluator.EvaluateAsync(productId, article.Id, at, cancellationToken);
                publishedByArticle.TryGetValue(article.Id, out var current);
                var fabricId = await FabricIdAsync(productId, article.Id, cancellationToken);
                articles.Add(preview.IsSuccess
                    ? new SewingArticlePriceStatus(article.Id, article.SaleSku, article.Active, fabricId,
                        current, preview.Value.Calculation)
                    : new SewingArticlePriceStatus(article.Id, article.SaleSku, article.Active, fabricId, current,
                        new SewingArticleCalculation(null, null, [new SewingPricingBlocker("MissingInputs", null, "Article inputs are unavailable.")])));
            }

            models.Add(new SewingModelPriceStatus(productId, list.ModelCode, articles));
        }

        return models;
    }

    /// <summary>The Article's own fabric, else the model's shared fabric: the sheet names an Article by it.</summary>
    private async Task<int?> FabricIdAsync(int productId, long articleId, CancellationToken cancellationToken)
    {
        var snapshot = await composition.ReadCompositionAsync(productId, articleId, cancellationToken);
        var fabrics = snapshot?.Materials.Where(x => x.Kind == FabricKind && x.ReferenceId is not null).ToList() ?? [];
        return (fabrics.FirstOrDefault(x => x.ArticleId == articleId) ?? fabrics.FirstOrDefault())?.ReferenceId;
    }
}

public sealed record SewingModelPriceStatus(int ProductId, string? ModelCode, IReadOnlyList<SewingArticlePriceStatus> Articles);

public sealed record SewingArticlePriceStatus(
    long ArticleId, string SaleSku, bool Active, int? FabricId, SewingPublishedArticlePrice? Published,
    SewingArticleCalculation Live)
{
    /// <summary>Active, calculable, and either never published or published with other prices.</summary>
    public bool NeedsPublication => Active && Live.IsCalculated && (Published is null || Published.Prices != Live.Prices);
}
