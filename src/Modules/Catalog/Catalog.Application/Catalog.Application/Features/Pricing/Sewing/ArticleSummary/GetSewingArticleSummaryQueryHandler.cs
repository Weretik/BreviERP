using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Application.Features.Product.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.ArticleSummary;

public sealed class GetSewingArticleSummaryQueryHandler(
    ISewingCompositionReadStore composition,
    ISewingPricingInputReader inputs,
    ISewingPricingSettingsReader settings,
    IPricingReferenceVersionReader references,
    ISewingPublishedPriceReader published,
    TimeProvider timeProvider)
    : IQueryHandler<GetSewingArticleSummaryQuery, Result<SewingArticleSummary>>
{
    public async ValueTask<Result<SewingArticleSummary>> Handle(
        GetSewingArticleSummaryQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductId <= 0)
            return Result.Invalid(new ValidationError("productId", "A positive Product ID is required."));
        var list = await composition.ReadArticlesAsync(query.ProductId, cancellationToken);
        if (list is null)
            return Result.NotFound();

        var at = timeProvider.GetUtcNow();
        var publishedByArticle = list.Articles.Count == 0
            ? new Dictionary<long, SewingPublishedArticlePrice>()
            : (await published.ReadAsync([query.ProductId], cancellationToken))
                .SelectMany(product => product.Articles).ToDictionary(article => article.ArticleId);
        var evaluator = new SewingArticlePricingEvaluator(inputs, settings, references);
        var rows = new List<SewingArticleSummaryRow>(list.Articles.Count);
        foreach (var article in list.Articles)
        {
            var preview = await evaluator.EvaluateAsync(query.ProductId, article.Id, at, cancellationToken);
            if (!preview.IsSuccess)
                return Result.NotFound();
            var calculation = preview.Value.Calculation;
            publishedByArticle.TryGetValue(article.Id, out var current);
            rows.Add(new SewingArticleSummaryRow(article.Id, article.SaleSku, article.Active, article.SortOrder,
                calculation.IsCalculated, calculation.Blockers,
                calculation.Breakdown,
                calculation.Prices,
                current is null ? null : new SewingArticlePublishedSummary(current.PublishedAt, current.Prices)));
        }

        return Result.Success(new SewingArticleSummary(list.ProductId, list.ModelCode, list.Version, at, rows));
    }
}
