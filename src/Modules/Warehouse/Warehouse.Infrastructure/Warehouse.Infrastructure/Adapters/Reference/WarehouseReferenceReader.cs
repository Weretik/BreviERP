using Reference.Contracts.FabricColors;
using Reference.Contracts.Fabrics;
using Reference.Contracts.Suppliers;

using Warehouse.Application.Abstractions;

namespace Warehouse.Infrastructure.Adapters.Reference;

/// <summary>«Тканини», active «Постачальники» and active «Кольори» (Reference) for «Склад тканин».</summary>
internal sealed class WarehouseReferenceReader(IFabricDirectory fabrics, ISupplierDirectory suppliers,
    IFabricColorDirectory colors) : IWarehouseReferenceReader
{
    public async Task<IReadOnlyList<WarehouseFabric>> FabricsAsync(CancellationToken cancellationToken) =>
        (await fabrics.ListAsync(cancellationToken)).Select(x => new WarehouseFabric(x.Id, x.Code, x.Name, x.Price,
            x.Composition, x.Density, x.ProviderName, x.IsActive)).ToArray();

    public async Task<IReadOnlyList<WarehouseSupplier>> SuppliersAsync(CancellationToken cancellationToken) =>
        (await suppliers.ListAsync(cancellationToken)).Where(x => x.IsActive)
        .Select(x => new WarehouseSupplier(x.Id, x.Name)).ToArray();

    public async Task<IReadOnlyList<string>> ColorsAsync(CancellationToken cancellationToken) =>
        (await colors.ListAsync(cancellationToken)).Where(x => x.IsActive).Select(x => x.Name).ToArray();
}
