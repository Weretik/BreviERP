using Mediator;
using Reference.Contracts.Fabrics;
using Reference.Application.Features.Fabric.Queries.GetAllFabric;

namespace Reference.Infrastructure.Contracts.Fabrics;

internal sealed class FabricDirectory(ISender sender) : IFabricDirectory
{
    public async Task<IReadOnlyList<FabricInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllFabricQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference fabrics could not be read.");
        return result.Value.Select(x => new FabricInfo(x.Id, x.Code, x.Name, x.Price, x.Composition, x.Density, x.ProviderId, x.ProviderName,
                x.ProposalDescription, x.IsActive)).ToArray();
    }
}
