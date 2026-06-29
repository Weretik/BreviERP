using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByParentAndSlugSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByParentAndSlugSpec(int? parentId, string slug)
    {
        if (parentId.HasValue)
        {
            var categoryId = ProductCategoryId.From(parentId.Value);
            Query.Where(x => x.ParentId == categoryId && x.Slug == slug);
            return;
        }

        Query.Where(x => !x.ParentId.HasValue && x.Slug == slug);
    }
}
