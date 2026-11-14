using Production.Contracts.Orders;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Infrastructure.Adapters.Production;

/// <summary>Production orders of a КП (R-33).</summary>
internal sealed class QuoteProductionOrders(IQuoteOrderLinks links) : IQuoteProductionOrders
{
    public Task<IReadOnlySet<long>> WithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken) => links.QuotesWithOrdersAsync(quoteIds, cancellationToken);
}
