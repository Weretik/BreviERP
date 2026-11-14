using Crm.Application.Quotes.ProductionLinks;
using Production.Contracts.Orders;

namespace Crm.Infrastructure.Adapters.Production;

/// <summary>Production orders of a КП (R-33).</summary>
internal sealed class QuoteProductionOrders(IQuoteOrderLinks links) : IQuoteProductionOrders
{
    public Task<IReadOnlySet<long>> WithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken) => links.QuotesWithOrdersAsync(quoteIds, cancellationToken);
}
