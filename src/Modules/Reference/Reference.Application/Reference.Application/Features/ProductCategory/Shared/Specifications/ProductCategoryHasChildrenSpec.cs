using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryHasChildrenSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryHasChildrenSpec(int id)
    {
        var categoryId = ProductCategoryId.From(id);
        Query.Where(x => x.ParentId == categoryId);
    }
}
