using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared;

public sealed class ProductCategoryByIdSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByIdSpec(int id)
    {
        Query.Where(x => x.Id == ProductCategoryId.Create(id));
    }
}


