namespace Purchasing.Application.Abstractions;

/// <summary>«Постачальники» (Reference) for the manual supplier link; implemented in the host.</summary>
public interface IPurchasingSupplierReader
{
    /// <summary>All suppliers, inactive too, so an old link still shows its name.</summary>
    Task<IReadOnlyList<PurchasingSupplier>> ListAsync(CancellationToken cancellationToken);
}

public sealed record PurchasingSupplier(int Id, string Name, bool Active);
