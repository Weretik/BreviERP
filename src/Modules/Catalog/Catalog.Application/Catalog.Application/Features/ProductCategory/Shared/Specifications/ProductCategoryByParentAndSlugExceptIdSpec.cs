using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryByParentAndSlugExceptIdSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryByParentAndSlugExceptIdSpec(int id, int? parentId, string slug)
    {
        var excludedId = ProductCategoryId.Create(id);

        if (parentId.HasValue)
        {
            var categoryId = ProductCategoryId.Create(parentId.Value);
            Query.Where(x => x.Id != excludedId && x.ParentId == categoryId && x.Slug == slug);
            return;
        }

        Query.Where(x => x.Id != excludedId && !x.ParentId.HasValue && x.Slug == slug);
    }
}


