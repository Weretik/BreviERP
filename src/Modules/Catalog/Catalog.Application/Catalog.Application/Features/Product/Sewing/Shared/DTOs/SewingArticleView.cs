namespace Catalog.Application.Features.Product.Sewing;

/// <summary>A fabric variant of the model: its sale SKU and its fabric with metres per product (CAT-015).</summary>
public sealed record SewingArticleView(
    long Id,
    int ProductId,
    string SaleSku,
    bool Active,
    int SortOrder,
    int? FabricId,
    decimal? FabricQuantity);
