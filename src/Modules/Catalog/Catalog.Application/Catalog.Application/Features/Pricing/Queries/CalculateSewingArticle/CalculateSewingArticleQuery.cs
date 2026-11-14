using Catalog.Application.Abstractions.Pricing;
namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingArticle;

public sealed record CalculateSewingArticleQuery(int ProductId, long ArticleId, DateTimeOffset At)
    : IQuery<Result<SewingArticlePricePreview>>;

