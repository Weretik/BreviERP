using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;

public sealed record GetPublishedSewingProductsQuery(IReadOnlyList<int> ProductIds)
    : IQuery<Result<IReadOnlyList<SewingPublishedProductPrices>>>;
