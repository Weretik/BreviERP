using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetCurrent;

public sealed class GetPublishedSewingPriceQueryHandler(ISewingPublicationStore store)
    : IQueryHandler<GetPublishedSewingPriceQuery, Result<PublishedSewingPrice>>
{
    public async ValueTask<Result<PublishedSewingPrice>> Handle(
        GetPublishedSewingPriceQuery query, CancellationToken cancellationToken)
    {
        var price = await store.ReadCurrentAsync(query.ProductId, query.ArticleId, cancellationToken);
        return price is null ? Result.NotFound() : Result.Success(price);
    }
}
