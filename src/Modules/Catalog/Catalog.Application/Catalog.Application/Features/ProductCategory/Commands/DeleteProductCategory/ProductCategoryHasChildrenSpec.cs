using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

using Catalog.Application.Features.ProductCategory.Shared;

namespace Catalog.Application.Features.ProductCategory.Commands.DeleteProductCategory;

public sealed class ProductCategoryHasChildrenSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryHasChildrenSpec(int id)
    {
        var categoryId = ProductCategoryId.Create(id);
        Query.Where(x => x.ParentId == categoryId);
    }
}


