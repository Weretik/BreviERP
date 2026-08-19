using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.Shared.Specifications;
using Catalog.Application.Features.Product.GetAdminList.Specifications;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminList;

public sealed class GetAdminProductsQueryHandler(
    ICatalogReadRepository<ProductEntity> repository,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository) : IQueryHandler<GetAdminProductsQuery, PagedResult<IReadOnlyList<ProductListItem>>>
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

        return CreatePagedResult(rows, page, pageSize, total);
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
