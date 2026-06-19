using Reference.Domain.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

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
