namespace Reference.Contracts.GarmentAccessories;

/// <summary>«Фурнітура» for other modules.</summary>
public interface IGarmentAccessoryDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<GarmentAccessoryInfo>> ListAsync(CancellationToken cancellationToken);
}
