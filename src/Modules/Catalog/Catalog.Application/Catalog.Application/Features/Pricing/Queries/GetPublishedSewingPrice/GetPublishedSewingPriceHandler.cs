using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Publications;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingPrice;

public sealed class GetPublishedSewingPriceHandler(ISewingPublicationStore store)
    : IQueryHandler<GetPublishedSewingPriceQuery, Result<PublishedSewingPrice>>
{
    public async ValueTask<Result<PublishedSewingPrice>> Handle(
        GetPublishedSewingPriceQuery query, CancellationToken cancellationToken)
    {
        var price = await store.ReadCurrentAsync(query.ProductId, query.ArticleId, cancellationToken);
        return price is null ? Result.NotFound() : Result.Success(price);
    }
}
