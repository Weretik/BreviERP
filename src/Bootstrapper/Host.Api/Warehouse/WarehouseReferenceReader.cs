using Mediator;
using Reference.Application.Features.Fabric.GetList;
using Reference.Application.Features.FabricColor.GetList;
using Reference.Application.Features.Supplier.GetList;
using Warehouse.Application.Abstractions;

namespace Host.Api.Warehouse;

/// <summary>«Тканини», active «Постачальники» and active «Кольори» (Reference) for «Склад тканин».</summary>
internal sealed class WarehouseReferenceReader(ISender sender) : IWarehouseReferenceReader
{
    public async Task<IReadOnlyList<WarehouseFabric>> FabricsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllFabricQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference fabrics could not be read.");
        return result.Value.Select(x => new WarehouseFabric(x.Id, x.Code, x.Name, x.Price)).ToArray();
    }

    public async Task<IReadOnlyList<WarehouseSupplier>> SuppliersAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSuppliersQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference suppliers could not be read.");
        return result.Value.Where(x => x.IsActive).Select(x => new WarehouseSupplier(x.Id, x.Name)).ToArray();
    }

    public async Task<IReadOnlyList<string>> ColorsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFabricColorsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference colours could not be read.");
        return result.Value.Where(x => x.IsActive).Select(x => x.Name).ToArray();
    }
}
