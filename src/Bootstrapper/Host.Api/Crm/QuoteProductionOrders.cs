using Crm.Application.Quotes.ProductionLinks;
using Mediator;
using Production.Application.Orders.QuoteSources;

namespace Host.Api.Crm;

/// <summary>Production orders of a КП, read through the Production query (R-33).</summary>
internal sealed class QuoteProductionOrders(ISender sender) : IQuoteProductionOrders
{
    public async Task<IReadOnlySet<long>> WithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotesWithOrdersQuery(quoteIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Production orders could not be read.");
        return result.Value.ToHashSet();
    }
}
