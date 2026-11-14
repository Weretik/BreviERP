using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared;

public sealed class ProductCategoryHasChildrenSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryHasChildrenSpec(int id)
    {
        var categoryId = ProductCategoryId.Create(id);
        Query.Where(x => x.ParentId == categoryId);
    }
}


