namespace Warehouse.Application.Abstractions;

/// <summary>«Тканини», «Постачальники» and «Кольори» (Reference) for the stock; implemented in the host.</summary>
public interface IWarehouseReferenceReader
{
    Task<IReadOnlyList<WarehouseFabric>> FabricsAsync(CancellationToken cancellationToken);

    /// <summary>Active suppliers.</summary>
    Task<IReadOnlyList<WarehouseSupplier>> SuppliersAsync(CancellationToken cancellationToken);

    /// <summary>Active colours in list order.</summary>
    Task<IReadOnlyList<string>> ColorsAsync(CancellationToken cancellationToken);
}

/// <param name="Price">Price per metre of «Тканини», UAH.</param>
/// <param name="Supplier">Name of the fabric's supplier in «Тканини».</param>
public sealed record WarehouseFabric(int Id, string? Code, string Name, decimal? Price = null,
    string? Composition = null, string? Density = null, string? Supplier = null, bool IsActive = true);

public sealed record WarehouseSupplier(int Id, string Name);
