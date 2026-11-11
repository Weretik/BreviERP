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

/// <param name="Price">Price per metre of «Тканини», UAH; values the stock.</param>
public sealed record WarehouseFabric(int Id, string? Code, string Name, decimal? Price = null);

public sealed record WarehouseSupplier(int Id, string Name);
