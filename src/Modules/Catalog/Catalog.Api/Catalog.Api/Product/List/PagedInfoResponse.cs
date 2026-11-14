namespace Catalog.Api.Product.List;

public sealed record PagedInfoResponse(
    long PageNumber,
    long PageSize,
    long TotalPages,
    long TotalRecords);
