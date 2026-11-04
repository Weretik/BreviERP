namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.List;

public sealed record GetPpePriceSourcesQuery : IQuery<Result<IReadOnlyList<PpePriceSourceView>>>;
