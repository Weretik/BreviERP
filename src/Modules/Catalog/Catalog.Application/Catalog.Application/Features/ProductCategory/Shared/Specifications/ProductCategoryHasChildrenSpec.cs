using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryHasChildrenSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryHasChildrenSpec(int id)
    {
        var categoryId = ProductCategoryId.Create(id);
        Query.Where(x => x.ParentId == categoryId);
    }
}


