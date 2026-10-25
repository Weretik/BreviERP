using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.Shared.Specifications;
using Catalog.Application.Features.Product.GetAdminList.Specifications;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminList;

public sealed class GetAdminProductsQueryHandler(
    ICatalogReadRepository<ProductEntity> repository,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository,
    ICatalogReadRepository<MediaFile> mediaRepository) : IQueryHandler<GetAdminProductsQuery, PagedResult<IReadOnlyList<ProductListItem>>>
{
    public async ValueTask<PagedResult<IReadOnlyList<ProductListItem>>> Handle(GetAdminProductsQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page;
        var pageSize = query.PageSize;
        IReadOnlyCollection<ProductCategoryId>? categoryIds = null;

        if (query.CategoryId.HasValue)
        {
            var category = await categoryRepository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(query.CategoryId.Value), cancellationToken);
            if (category is null)
                return CreatePagedResult([], page, pageSize, 0);

            var descendants = await categoryRepository.ListAsync(
                new ProductCategoryDescendantsByPathSpec(category.Id.Value, category.Path), cancellationToken);
            categoryIds = [category.Id, .. descendants.Select(x => x.Id)];
        }

        var filter = new GetAdminProductsSpec(query.Search, query.Type, categoryIds, query.SortBy, query.Descending);
        var total = await repository.CountAsync(filter, cancellationToken);
        var rows = await repository.ListAsync(
            new GetAdminProductsSpec(query.Search, query.Type, categoryIds, query.SortBy, query.Descending, page, pageSize),
            cancellationToken);

        if (rows.Count == 0)
            return CreatePagedResult([], page, pageSize, total);

        var mainPhotoIds = rows
            .Select(x => x.MainPhotoMediaFileId)
            .OfType<int>()
            .Distinct()
            .ToArray();
        var mediaUrls = mainPhotoIds.Length == 0
            ? new Dictionary<int, string>()
            : (await mediaRepository.ListAsync(new ProductMediaUrlsByIdsSpec(mainPhotoIds), cancellationToken))
                .ToDictionary(x => x.MediaFileId, x => x.Url);
        var items = rows.Select(row => new ProductListItem(
            row.Id,
            row.Name,
            row.Slug,
            row.Type,
            row.CategoryIds,
            row.MainPhotoMediaFileId is { } mediaFileId && mediaUrls.TryGetValue(mediaFileId, out var url)
                ? new ProductMainPhoto(mediaFileId, url)
                : null,
            0m,
            row.CreatedAtUtc,
            row.UpdatedAtUtc)).ToList();

        return CreatePagedResult(items, page, pageSize, total);
    }

    private static PagedResult<IReadOnlyList<ProductListItem>> CreatePagedResult(
        IReadOnlyList<ProductListItem> items,
        int page,
        int pageSize,
        int totalRecords)
        => new(
            new PagedInfo(page, pageSize, (long)Math.Ceiling(totalRecords / (double)pageSize), totalRecords),
            items);

}
