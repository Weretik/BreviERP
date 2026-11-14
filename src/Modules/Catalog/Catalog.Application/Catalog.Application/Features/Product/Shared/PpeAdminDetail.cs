
namespace Catalog.Application.Features.Product.Shared;

public sealed record PpeAdminDetail(
    NamedReferenceAdminDetail Supplier,
    decimal BasePrice,
    PpePercentAdminDetail RetailPercent,
    PpePercentAdminDetail WholesalePercent,
    decimal RetailPrice,
    decimal WholesalePrice);
