using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByNameSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByNameSpec(string name)
    {
        Query.Where(x => x.Name == name);
    }
}
