namespace Catalog.Application.Abstractions.Products;

/// <summary>
/// Reads the fabric variants (sewing Articles) of the given models for the «Товари» list in one query: the fabric of
/// each variant and its current published prices (CAT-013).
/// </summary>
public interface IProductOverviewVariantReader
{
    Task<IReadOnlyList<ProductOverviewVariantRow>> ReadAsync(
        IReadOnlyCollection<int> productIds, CancellationToken cancellationToken);
}
