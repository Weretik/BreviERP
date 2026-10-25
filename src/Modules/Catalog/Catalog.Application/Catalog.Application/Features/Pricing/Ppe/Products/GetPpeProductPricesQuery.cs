using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Products;
public sealed record GetPpeProductPricesQuery(IReadOnlyList<int> ProductIds) : IQuery<Result<IReadOnlyList<PpeProductPricing>>>;
