using Reference.Domain.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryHasChildrenSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryHasChildrenSpec(int id)
    {
        var categoryId = ProductCategoryId.From(id);
        Query.Where(x => x.ParentId == categoryId);
    }
}
