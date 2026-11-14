namespace Catalog.Application.Abstractions.Reference;

public interface IProductReferenceReader
{
    Task<ProductReferenceData> GetSnapshotAsync(CancellationToken cancellationToken);
}
