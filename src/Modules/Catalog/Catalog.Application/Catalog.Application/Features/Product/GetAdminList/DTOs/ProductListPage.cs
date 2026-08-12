namespace Catalog.Application.Features.Product.GetAdminList.DTOs;

public sealed record ProductListPage(
    IReadOnlyList<ProductListItem> Items,
    int Page,
    int PageSize,
    int TotalCount);
