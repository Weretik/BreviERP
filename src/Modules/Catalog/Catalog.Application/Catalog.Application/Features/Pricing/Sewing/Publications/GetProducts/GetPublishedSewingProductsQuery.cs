using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;

public sealed record GetPublishedSewingProductsQuery(IReadOnlyList<int> ProductIds)
    : IQuery<Result<IReadOnlyList<SewingPublishedProductPrices>>>;
