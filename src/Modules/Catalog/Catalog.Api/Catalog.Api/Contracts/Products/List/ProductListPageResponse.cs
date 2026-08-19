namespace Catalog.Api.Contracts.Products;

public sealed record ProductListPageResponse(
    IReadOnlyList<ProductListResponse> Value,
    PagedInfoResponse PagedInfo);
