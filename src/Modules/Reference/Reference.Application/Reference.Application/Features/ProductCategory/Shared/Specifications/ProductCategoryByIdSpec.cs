using Reference.Domain.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByIdSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByIdSpec(int id)
    {
        Query.Where(x => x.Id == ProductCategoryId.From(id));
    }
}
