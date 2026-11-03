namespace Catalog.Application.Contracts.Reference;

/// <summary>
/// Reads the Reference codes and names that the Calculator sheet uses (FAB-…, FUR-…, OP-…, supplier and section
/// names), so that Catalog can turn them into reference IDs without a database relation.
/// </summary>
public interface IProductReferenceCodeReader
{
    Task<ProductReferenceCodes> ReadAsync(CancellationToken cancellationToken);
}
