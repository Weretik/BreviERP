namespace Catalog.Application.Abstractions.Products;

/// <param name="FabricId">The variant's own fabric line, else the model's shared fabric line.</param>
/// <param name="Retail">Null when the variant has never been published.</param>
public sealed record ProductOverviewVariantRow(
    int ProductId,
    long ArticleId,
    string SaleSku,
    bool IsActive,
    int SortOrder,
    int? FabricId,
    decimal? Retail,
    decimal? Wholesale,
    decimal? LargeWholesale,
    DateTimeOffset? PublishedAt);
