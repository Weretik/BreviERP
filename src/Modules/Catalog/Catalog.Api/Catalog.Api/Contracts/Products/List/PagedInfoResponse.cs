namespace Catalog.Api.Contracts.Products;

public sealed record PagedInfoResponse(
    long PageNumber,
    long PageSize,
    long TotalPages,
    long TotalRecords);
