using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;

public sealed class GetStoreProductCategoriesSpec : Specification<ProductCategoryEntity, StoreProductCategoryCandidateDTO>
{
    public GetStoreProductCategoriesSpec(string language)
    {
        var useRuName = language == "ru";

        Query.AsNoTracking()
            .OrderBy(x => x.Path)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new StoreProductCategoryCandidateDTO(
                x.Id.Value,
                useRuName ? x.RuName : x.Name,
                x.Slug,
                x.ParentId.HasValue ? x.ParentId.Value.Value : null,
                x.Path,
                x.Level,
                x.IsActive));
    }
}


