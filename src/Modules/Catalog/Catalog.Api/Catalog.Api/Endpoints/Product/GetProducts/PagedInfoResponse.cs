namespace Catalog.Api.Endpoints.Product.GetProducts;

public sealed record PagedInfoResponse(
    long PageNumber,
    long PageSize,
    long TotalPages,
    long TotalRecords);
