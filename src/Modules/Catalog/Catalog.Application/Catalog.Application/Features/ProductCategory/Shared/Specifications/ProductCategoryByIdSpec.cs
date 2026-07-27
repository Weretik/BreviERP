using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByIdSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByIdSpec(int id)
    {
        Query.Where(x => x.Id == ProductCategoryId.Create(id));
    }
}


