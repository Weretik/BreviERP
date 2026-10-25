using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetCurrent;

public sealed record GetPublishedSewingPriceQuery(int ProductId, long ArticleId) : IQuery<Result<PublishedSewingPrice>>;
