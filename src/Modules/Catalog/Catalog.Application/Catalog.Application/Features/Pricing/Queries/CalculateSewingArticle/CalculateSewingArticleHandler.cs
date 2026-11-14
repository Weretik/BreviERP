using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared.Sewing.Evaluation;
using Catalog.Application.Abstractions.Pricing.Calculations;
using Catalog.Application.Abstractions.Pricing.Inputs;
using Catalog.Application.Abstractions.Pricing.Settings;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingArticle;

public sealed class CalculateSewingArticleHandler(
    ISewingPricingInputReader inputs, ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : IQueryHandler<CalculateSewingArticleQuery, Result<SewingArticlePricePreview>>
{
    public ValueTask<Result<SewingArticlePricePreview>> Handle(
        CalculateSewingArticleQuery query, CancellationToken cancellationToken)
        => new SewingArticlePricingEvaluator(inputs, settings, references)
            .EvaluateAsync(query.ProductId, query.ArticleId, query.At, cancellationToken);
}
