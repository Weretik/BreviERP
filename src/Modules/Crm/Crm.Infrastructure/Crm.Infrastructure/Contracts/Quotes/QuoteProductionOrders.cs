using Crm.Contracts.Quotes;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Infrastructure.Contracts.Quotes;

/// <summary>Production orders of a КП (R-33), from every module that implements <see cref="IQuoteOrderLinks"/>.</summary>
internal sealed class QuoteProductionOrders(IEnumerable<IQuoteOrderLinks> sources) : IQuoteProductionOrders
{
    public async Task<IReadOnlySet<long>> WithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken)
    {
        var withOrders = new HashSet<long>();
        foreach (var source in sources)
            withOrders.UnionWith(await source.QuotesWithOrdersAsync(quoteIds, cancellationToken));
        return withOrders;
    }
}
