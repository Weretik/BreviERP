using Mediator;
using Production.Application.Abstractions;
using Reference.Application.Features.Fabric.GetList;

namespace Host.Api.Production;

/// <summary>Fabrics of «Тканини» (Reference) for the codes and names an order item keeps.</summary>
internal sealed class ProductionFabricReader(ISender sender) : IProductionFabricReader
{
    public async Task<IReadOnlyList<ProductionFabric>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllFabricQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference fabrics could not be read.");
        return result.Value.Select(x => new ProductionFabric(x.Id, x.Code, x.Name, x.Price)).ToArray();
    }
}
