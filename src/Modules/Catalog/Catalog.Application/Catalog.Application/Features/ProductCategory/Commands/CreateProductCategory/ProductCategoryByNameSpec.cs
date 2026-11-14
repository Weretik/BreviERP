using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

using Catalog.Application.Features.ProductCategory.Shared;

namespace Catalog.Application.Features.ProductCategory.Commands.CreateProductCategory;

public sealed class ProductCategoryByNameSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}


