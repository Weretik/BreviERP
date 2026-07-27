using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryDescendantsByPathSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoryDescendantsByPathSpec(int id, string path)
    {
        var categoryId = ProductCategoryId.Create(id);

        Query
            .Where(x => x.Id != categoryId && x.Path.StartsWith(path))
            .OrderBy(x => x.Path);
    }
}


