using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public class ProductCategoryReference : BaseEntity<ProductCategoryId>
{
    public ProductId ProductId { get; private set; }
    public ProductCategoryId CategoryId => Id;

    private ProductCategoryReference() { }

    private ProductCategoryReference(ProductId productId, ProductCategoryId categoryId)
    {
        if (productId.Value == default)
            throw new DomainException(ProductErrors.IdIsRequired());

        ProductId = productId;
        SetId(categoryId);
    }

    public static ProductCategoryReference Create(ProductId productId, ProductCategoryId categoryId) => new(productId, categoryId);

    private void SetId(ProductCategoryId categoryId)
    {
        if (categoryId.Value == default)
            throw new DomainException(ProductErrors.CategoryIdIsRequired());

        Id = categoryId;
    }
}
