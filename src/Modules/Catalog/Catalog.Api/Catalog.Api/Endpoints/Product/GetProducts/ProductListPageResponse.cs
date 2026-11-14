namespace Catalog.Api.Endpoints.Product.GetProducts;

public sealed record ProductListPageResponse(
    IReadOnlyList<ProductListResponse> Value,
    PagedInfoResponse PagedInfo);
