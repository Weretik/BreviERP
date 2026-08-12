namespace Catalog.Application.Contracts.Reference;

public interface IProductReferenceReader
{
    Task<ProductReferenceData> GetSnapshotAsync(CancellationToken cancellationToken);
}
