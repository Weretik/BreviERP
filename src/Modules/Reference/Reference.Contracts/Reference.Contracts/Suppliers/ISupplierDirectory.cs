namespace Reference.Contracts.Suppliers;

/// <summary>«Постачальники» for other modules.</summary>
public interface ISupplierDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<SupplierInfo>> ListAsync(CancellationToken cancellationToken);
}
