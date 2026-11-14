using Catalog.Api.Endpoints.Product.GetProducts;

namespace Catalog.Api.Endpoints.Shared;

public sealed record PagedInfoResponse(
    long PageNumber,
    long PageSize,
    long TotalPages,
    long TotalRecords);
