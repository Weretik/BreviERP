namespace Reference.Contracts.FabricColors;

/// <summary>«Кольори» for other modules.</summary>
public interface IFabricColorDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<FabricColorInfo>> ListAsync(CancellationToken cancellationToken);
}
