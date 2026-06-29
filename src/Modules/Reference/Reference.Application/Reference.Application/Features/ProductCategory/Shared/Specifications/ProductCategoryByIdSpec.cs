using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByIdSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByIdSpec(int id)
    {
        Query.Where(x => x.Id == ProductCategoryId.From(id));
    }
}
