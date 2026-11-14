using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByNameSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}


