using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared;

public sealed class ProductCategoryByParentAndSlugSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByParentAndSlugSpec(int? parentId, string slug)
    {
        if (parentId.HasValue)
        {
            var categoryId = ProductCategoryId.Create(parentId.Value);
            Query.Where(x => x.ParentId == categoryId && x.Slug == slug);
            return;
        }

        Query.Where(x => !x.ParentId.HasValue && x.Slug == slug);
    }
}


