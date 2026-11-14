namespace Reference.Contracts.GarmentPartOperations;

/// <summary>«Операції» for other modules.</summary>
public interface IGarmentPartOperationDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<GarmentPartOperationInfo>> ListAsync(CancellationToken cancellationToken);
}
