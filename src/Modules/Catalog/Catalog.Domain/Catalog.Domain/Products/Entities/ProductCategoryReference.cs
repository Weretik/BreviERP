using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.Entities;

public class ProductCategoryReference : BaseEntity<ProductCategoryId>
{
    public ProductCategoryId CategoryId => Id;

    private ProductCategoryReference() { }

    private ProductCategoryReference(ProductCategoryId categoryId)
    {
        SetId(categoryId);
    }

    public static ProductCategoryReference Create(ProductCategoryId categoryId) => new(categoryId);

    private void SetId(ProductCategoryId categoryId)
    {
        if (categoryId.Value == default)
            throw new DomainException(ProductErrors.CategoryIdIsRequired());

        Id = categoryId;
    }
}