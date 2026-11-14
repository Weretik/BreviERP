using Crm.Application.Quotes.FabricFacts;

using Reference.Contracts.Fabrics;

namespace Crm.Infrastructure.Adapters.Reference;

/// <summary>Reference fabrics as the manager line under a КП fabric needs them (R-31).</summary>
internal sealed class QuoteFabricFactsReader(IFabricDirectory fabrics) : IQuoteFabricFactsReader
{
    public async Task<IReadOnlyList<QuoteFabricFacts>> GetAsync(IReadOnlyCollection<int> fabricIds,
        CancellationToken cancellationToken) =>
        (await fabrics.ListAsync(cancellationToken)).Where(x => fabricIds.Contains(x.Id))
        .Select(x => new QuoteFabricFacts(x.Id, x.Name, x.Composition, x.Density, x.Price,
            x.ProviderId > 0 ? x.ProviderId : null, x.ProviderName))
        .ToArray();
}
