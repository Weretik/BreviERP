using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.GetAdminDetail.DTOs;

public sealed record PpeReadModel(
    int SupplierId,
    decimal BasePrice,
    PricePercentSource RetailSource,
    int? RetailAdditionalReferenceId,
    decimal? RetailCustomPercent,
    PricePercentSource WholesaleSource,
    int? WholesaleAdditionalReferenceId,
    decimal? WholesaleCustomPercent);
