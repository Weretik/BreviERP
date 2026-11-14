using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Calculations;
namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingArticle;

public sealed record CalculateSewingArticleQuery(int ProductId, long ArticleId, DateTimeOffset At)
    : IQuery<Result<SewingArticlePricePreview>>;

