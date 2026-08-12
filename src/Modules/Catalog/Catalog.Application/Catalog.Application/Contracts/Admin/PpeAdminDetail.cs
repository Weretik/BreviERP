namespace Catalog.Application.Contracts.Admin;

public sealed record PpeAdminDetail(
    int SupplierId,
    decimal BasePrice,
    int? RetailReferenceId,
    decimal? RetailCustomPercent,
    int? WholesaleReferenceId,
    decimal? WholesaleCustomPercent);
