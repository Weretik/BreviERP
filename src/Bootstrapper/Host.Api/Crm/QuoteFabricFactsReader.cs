using Crm.Application.Quotes.FabricFacts;
using Mediator;
using Reference.Application.Features.Fabric.GetList;

namespace Host.Api.Crm;

/// <summary>Reference fabrics as the manager line under a КП fabric needs them (R-31).</summary>
internal sealed class QuoteFabricFactsReader(ISender sender) : IQuoteFabricFactsReader
{
    public async Task<IReadOnlyList<QuoteFabricFacts>> GetAsync(IReadOnlyCollection<int> fabricIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllFabricQuery(), cancellationToken);
        if (!result.IsSuccess) return [];
        return result.Value.Where(x => fabricIds.Contains(x.Id))
            .Select(x => new QuoteFabricFacts(x.Id, x.Name, x.Composition, x.Density, x.Price,
                x.ProviderId > 0 ? x.ProviderId : null, x.ProviderName))
            .ToArray();
    }
}
