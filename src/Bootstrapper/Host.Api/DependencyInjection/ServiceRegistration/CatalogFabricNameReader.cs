using Catalog.Application.Contracts.Reference;
using Mediator;
using Reference.Application.Features.Fabric.GetList;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Gives Catalog the Reference fabric names through the Reference query (CAT-013).</summary>
internal sealed class CatalogFabricNameReader(ISender sender) : IFabricNameReader
{
    public async Task<IReadOnlyDictionary<int, string>> ReadAsync(CancellationToken cancellationToken)
        => (await sender.Send(new GetAllFabricQuery(), cancellationToken)).Value?.ToDictionary(x => x.Id, x => x.Name)
            ?? new Dictionary<int, string>();
}
