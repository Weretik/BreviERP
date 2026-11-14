using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

using Catalog.Application.Features.ProductCategory.Shared;

namespace Catalog.Application.Features.ProductCategory.Commands.CreateProductCategory;

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


