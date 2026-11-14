using Catalog.Domain.ProductCategories;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.ProductCategory.Shared.Specifications;

public sealed class ProductCategoryIsUsedByProductSpec : Specification<ProductEntity>
{
    public ProductCategoryIsUsedByProductSpec(int categoryId)
    {
        var productCategoryId = ProductCategoryId.Create(categoryId);

        Query.Where(x => x.Categories.Any(category => category.Id == productCategoryId));
    }
}
