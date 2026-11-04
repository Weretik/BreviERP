namespace Catalog.Application.Contracts.Persistence.ProductOverview;

/// <summary>
/// Reads the fabric variants (sewing Articles) of the given models for the «Товари» list in one query: the fabric of
/// each variant and its current published prices (CAT-013).
/// </summary>
public interface IProductOverviewVariantReader
{
    Task<IReadOnlyList<ProductOverviewVariantRow>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}

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
