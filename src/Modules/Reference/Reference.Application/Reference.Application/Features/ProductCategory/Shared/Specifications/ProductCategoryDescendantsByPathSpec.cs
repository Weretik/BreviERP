using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryDescendantsByPathSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryDescendantsByPathSpec(int id, string path)
    {
        var categoryId = ProductCategoryId.From(id);

        Query
            .Where(x => x.Id != categoryId && x.Path.StartsWith(path))
            .OrderBy(x => x.Path);
    }
}
