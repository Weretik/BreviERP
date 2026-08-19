using Ardalis.Specification;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.Product.Update.Specifications;

public sealed class ProductCategoriesByIdsSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoriesByIdsSpec(IEnumerable<int> ids)
    {
        var categoryIds = ids.Distinct().ToArray();
        Query.AsNoTracking().Where(x => categoryIds.Contains(x.Id.Value));
    }
}
