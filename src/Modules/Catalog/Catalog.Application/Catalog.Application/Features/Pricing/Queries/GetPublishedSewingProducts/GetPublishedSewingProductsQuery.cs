using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Publications;

namespace Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;

public sealed record GetPublishedSewingProductsQuery(IReadOnlyList<int> ProductIds)
    : IQuery<Result<IReadOnlyList<SewingPublishedProductPrices>>>;
