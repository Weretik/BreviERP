namespace Catalog.Contracts.Products;

/// <summary>Which library photo a product card uses.</summary>
public sealed record CatalogMediaUsage(int MediaFileId, int ProductId, string ProductName);

/// <summary>What of other modules the Catalog products use, so that it is not deleted.</summary>
public interface ICatalogUsage
{
    Task<IReadOnlyList<CatalogMediaUsage>> MediaUsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken);

    /// <summary>A product composition holds the Reference fabric.</summary>
    Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken);

    Task<bool> IsAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken);

    Task<bool> IsOperationUsedAsync(int operationId, CancellationToken cancellationToken);
}
