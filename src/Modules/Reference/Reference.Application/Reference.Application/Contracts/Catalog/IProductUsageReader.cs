namespace Reference.Application.Contracts.Catalog;

/// <summary>Catalog-owned read boundary used before Reference deletes an item.</summary>
public interface IProductUsageReader
{
    Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken);
    Task<bool> IsGarmentAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken);
    Task<bool> IsGarmentPartOperationUsedAsync(int operationId, CancellationToken cancellationToken);
}
