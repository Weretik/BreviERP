using Catalog.Application.Contracts.Pricing;
namespace Catalog.Application.Features.Pricing.Sewing.Preview;

public sealed record CalculateSewingArticleQuery(int ProductId, long ArticleId, DateTimeOffset At)
    : IQuery<Result<SewingArticlePricePreview>>;

