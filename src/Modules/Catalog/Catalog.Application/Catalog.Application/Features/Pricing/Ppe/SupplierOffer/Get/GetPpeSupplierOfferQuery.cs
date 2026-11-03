using Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Shared;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierOffer.Get;

public sealed record GetPpeSupplierOfferQuery(int ProductId) : IQuery<Result<PpeSupplierOffer>>;
