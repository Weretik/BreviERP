using Reference.Application.Features.ProductCategory.GetStoreList.DTOs;
using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.GetStoreList.Specifications;

public sealed class GetStoreProductCategoriesSpec : Specification<ProductCategoryEntity, StoreProductCategoryRowDTO>
{
    public GetStoreProductCategoriesSpec(string language)
    {
        var useRuName = language == "ru";

        Query.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Path)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new StoreProductCategoryRowDTO(
                x.Id.Value,
                useRuName ? x.RuName : x.Name,
                x.Slug,
                x.ParentId.HasValue ? x.ParentId.Value.Value : null,
                x.Level));
    }
}
