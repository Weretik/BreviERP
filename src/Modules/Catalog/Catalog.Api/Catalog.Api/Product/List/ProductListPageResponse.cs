namespace Catalog.Api.Product.List;

public sealed record ProductListPageResponse(
    IReadOnlyList<ProductListResponse> Value,
    PagedInfoResponse PagedInfo);
