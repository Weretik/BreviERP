using Catalog.Application.Features.Pricing.Shared;
namespace Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;
public sealed record GetPpeProductPricesQuery(IReadOnlyList<int> ProductIds) : IQuery<Result<IReadOnlyList<PpeProductPricing>>>;
