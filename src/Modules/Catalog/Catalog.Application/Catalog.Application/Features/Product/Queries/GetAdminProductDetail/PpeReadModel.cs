using Catalog.Domain.Products;
using Catalog.Domain.Products.Ppe.Pricing;

namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed record PpeReadModel(
    int SupplierId,
    decimal BasePrice,
    PricePercentSource RetailSource,
    int? RetailAdditionalReferenceId,
    decimal? RetailCustomPercent,
    PricePercentSource WholesaleSource,
    int? WholesaleAdditionalReferenceId,
    decimal? WholesaleCustomPercent);
