using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingPrice;

public sealed record GetPublishedSewingPriceQuery(int ProductId, long ArticleId) : IQuery<Result<PublishedSewingPrice>>;
