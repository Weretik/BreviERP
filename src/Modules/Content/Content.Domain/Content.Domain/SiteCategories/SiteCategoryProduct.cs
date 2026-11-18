using BuildingBlocks.Domain.Identifiers.Products;
using BuildingBlocks.Domain.Entity;
namespace Content.Domain.SiteCategories;

/// <summary>A Catalog product placed in a site category (CNT-003 R-03); no database link across modules.</summary>
public sealed class SiteCategoryProduct : IAggregateRoot
{
    private SiteCategoryProduct() { }

    private SiteCategoryProduct(SiteCategoryId categoryId, ProductId productId, int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId.Value);
        CategoryId = categoryId;
        ProductId = productId;
        SortOrder = sortOrder;
    }

    public static SiteCategoryProduct Create(SiteCategoryId categoryId, ProductId productId, int sortOrder) =>
        new(categoryId, productId, sortOrder);

    public SiteCategoryId CategoryId { get; private set; }
    public ProductId ProductId { get; private set; }

    /// <summary>Place of the product inside the category.</summary>
    public int SortOrder { get; private set; }
}
