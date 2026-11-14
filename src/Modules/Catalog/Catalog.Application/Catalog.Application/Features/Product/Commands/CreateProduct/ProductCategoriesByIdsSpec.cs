using Ardalis.Specification;
using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed class ProductCategoriesByIdsSpec : Specification<ProductCategoryEntity>
{
    public ProductCategoriesByIdsSpec(IEnumerable<int> ids)
    {
        // Compare typed ids: EF cannot translate `.Id.Value` of a converted key inside Contains.
        var categoryIds = ids.Distinct().Select(ProductCategoryId.Create).ToArray();
        Query.AsNoTracking().Where(x => categoryIds.Contains(x.Id));
    }
}
