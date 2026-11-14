namespace Reference.Contracts.Fabrics;

/// <summary>«Тканини» for other modules.</summary>
public interface IFabricDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<FabricInfo>> ListAsync(CancellationToken cancellationToken);
}
