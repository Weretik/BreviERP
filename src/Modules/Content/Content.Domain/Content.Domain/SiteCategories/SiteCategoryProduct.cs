using BuildingBlocks.Domain.Entity;
namespace Content.Domain.SiteCategories;

/// <summary>A Catalog product placed in a site category (CNT-003 R-03); no database link across modules.</summary>
public sealed class SiteCategoryProduct : IAggregateRoot
{
    private SiteCategoryProduct() { }

    private SiteCategoryProduct(int categoryId, int productId, int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        CategoryId = categoryId;
        ProductId = productId;
        SortOrder = sortOrder;
    }

    public static SiteCategoryProduct Create(int categoryId, int productId, int sortOrder) =>
        new(categoryId, productId, sortOrder);

    public int CategoryId { get; private set; }
    public int ProductId { get; private set; }

    /// <summary>Place of the product inside the category.</summary>
    public int SortOrder { get; private set; }
}
