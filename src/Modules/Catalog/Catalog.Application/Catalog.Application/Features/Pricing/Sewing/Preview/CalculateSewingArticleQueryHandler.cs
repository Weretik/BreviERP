using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;

namespace Catalog.Application.Features.Pricing.Sewing.Preview;

public sealed class CalculateSewingArticleQueryHandler(
    ISewingPricingInputReader inputs, ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : IQueryHandler<CalculateSewingArticleQuery, Result<SewingArticlePricePreview>>
{
    public ValueTask<Result<SewingArticlePricePreview>> Handle(
        CalculateSewingArticleQuery query, CancellationToken cancellationToken)
        => new SewingArticlePricingEvaluator(inputs, settings, references)
            .EvaluateAsync(query.ProductId, query.ArticleId, query.At, cancellationToken);
}
