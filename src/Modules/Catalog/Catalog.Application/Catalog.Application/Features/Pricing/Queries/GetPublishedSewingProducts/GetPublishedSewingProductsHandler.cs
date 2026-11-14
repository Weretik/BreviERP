using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Publications;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;

public sealed class GetPublishedSewingProductsHandler(ISewingPublishedPriceReader reader)
    : IQueryHandler<GetPublishedSewingProductsQuery, Result<IReadOnlyList<SewingPublishedProductPrices>>>
{
    public async ValueTask<Result<IReadOnlyList<SewingPublishedProductPrices>>> Handle(
        GetPublishedSewingProductsQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await reader.ReadAsync(query.ProductIds, cancellationToken));
    }
}
