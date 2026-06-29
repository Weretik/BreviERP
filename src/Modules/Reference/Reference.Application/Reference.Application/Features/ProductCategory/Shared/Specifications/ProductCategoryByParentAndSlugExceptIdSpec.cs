using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByParentAndSlugExceptIdSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByParentAndSlugExceptIdSpec(int id, int? parentId, string slug)
    {
        var excludedId = ProductCategoryId.From(id);

        if (parentId.HasValue)
        {
            var categoryId = ProductCategoryId.From(parentId.Value);
            Query.Where(x => x.Id != excludedId && x.ParentId == categoryId && x.Slug == slug);
            return;
        }

        Query.Where(x => x.Id != excludedId && !x.ParentId.HasValue && x.Slug == slug);
    }
}
