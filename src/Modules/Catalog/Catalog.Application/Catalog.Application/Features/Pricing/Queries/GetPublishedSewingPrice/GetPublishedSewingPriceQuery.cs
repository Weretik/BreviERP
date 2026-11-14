using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Publications;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingPrice;

public sealed record GetPublishedSewingPriceQuery(int ProductId, long ArticleId) : IQuery<Result<PublishedSewingPrice>>;
