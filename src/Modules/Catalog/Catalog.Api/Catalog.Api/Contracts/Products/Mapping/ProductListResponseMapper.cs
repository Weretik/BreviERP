using Ardalis.Result;
using Catalog.Application.Features.Product.GetAdminList.DTOs;

namespace Catalog.Api.Contracts.Products;

public static class ProductListResponseMapper
{
    public static ProductListPageResponse ToResponse(PagedResult<IReadOnlyList<ProductListItem>> result)
        => new(result.Value.Select(ToResponse).ToList(), new PagedInfoResponse(
            result.PagedInfo.PageNumber,
            result.PagedInfo.PageSize,
            result.PagedInfo.TotalPages,
            result.PagedInfo.TotalRecords));

    public static ProductListResponse ToResponse(ProductListItem item)
        => new(item.Id, item.Name, item.Slug, item.Type.ToString(), item.CategoryIds,
            item.MainPhoto is null ? null : new ProductMainPhotoResponse(item.MainPhoto.MediaFileId, item.MainPhoto.Url),
            item.MinimumWholesalePrice, item.CreatedAtUtc, item.UpdatedAtUtc);
}
