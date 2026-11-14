using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;

namespace Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;

public sealed record GetPpeSupplierOfferQuery(int ProductId) : IQuery<Result<PpeSupplierOffer>>;
