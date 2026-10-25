using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;

public sealed class GetPublishedSewingProductsQueryHandler(ISewingPublishedPriceReader reader)
    : IQueryHandler<GetPublishedSewingProductsQuery, Result<IReadOnlyList<SewingPublishedProductPrices>>>
{
    public async ValueTask<Result<IReadOnlyList<SewingPublishedProductPrices>>> Handle(
        GetPublishedSewingProductsQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductIds is null || query.ProductIds.Count is < 1 or > 100
            || query.ProductIds.Any(id => id <= 0) || query.ProductIds.Distinct().Count() != query.ProductIds.Count)
            return Result.Invalid(new ValidationError("productIds", "Provide between 1 and 100 unique positive Product IDs."));
        return Result.Success(await reader.ReadAsync(query.ProductIds, cancellationToken));
    }
}
