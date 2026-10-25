namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingArticleView(
    long Id,
    int ProductId,
    string SaleSku,
    bool Active,
    int SortOrder);

