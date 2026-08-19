using Catalog.Application.Contracts.Admin.Reference;

namespace Catalog.Application.Contracts.Admin.Ppe;

public sealed record PpeAdminDetail(
    NamedReferenceAdminDetail Supplier,
    decimal BasePrice,
    PpePercentAdminDetail RetailPercent,
    PpePercentAdminDetail WholesalePercent,
    decimal RetailPrice,
    decimal WholesalePrice);
