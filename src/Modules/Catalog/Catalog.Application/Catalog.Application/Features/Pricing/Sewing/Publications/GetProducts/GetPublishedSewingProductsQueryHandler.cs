using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;

public sealed class GetPublishedSewingProductsQueryHandler(ISewingPublishedPriceReader reader)
    : IQueryHandler<GetPublishedSewingProductsQuery, Result<IReadOnlyList<SewingPublishedProductPrices>>>
{
    public async ValueTask<Result<IReadOnlyList<SewingPublishedProductPrices>>> Handle(
        GetPublishedSewingProductsQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await reader.ReadAsync(query.ProductIds, cancellationToken));
    }
}
