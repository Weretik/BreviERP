using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.GetStoreList.DTOs;
using Catalog.Application.Features.ProductCategory.GetStoreList.Specifications;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.GetStoreList;

public sealed class GetStoreProductCategoriesQueryHandler(ICatalogReadRepository<ProductCategoryEntity> repository)
    : IQueryHandler<GetStoreProductCategoriesQuery, Result<List<StoreProductCategoryRowDTO>>>
{
    public async ValueTask<Result<List<StoreProductCategoryRowDTO>>> Handle(
        GetStoreProductCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var language = query.Language.Trim().ToLowerInvariant();
        var categories = await repository.ListAsync(new GetStoreProductCategoriesSpec(language), cancellationToken);
        var hiddenCategoryIds = new HashSet<int>();
        var result = new List<StoreProductCategoryRowDTO>();

        foreach (var category in categories)
        {
            var parentIsHidden = category.ParentId.HasValue && hiddenCategoryIds.Contains(category.ParentId.Value);
            if (!category.IsActive || parentIsHidden)
            {
                hiddenCategoryIds.Add(category.Id);
                continue;
            }

            result.Add(new StoreProductCategoryRowDTO(
                category.Id,
                category.Name,
                category.Slug,
                category.ParentId,
                category.Level));
        }

        if (result is { Count: 0 })
            return Result.NotFound();

        return Result.Success(result);
    }
}


