using Mediator;
using Production.Contracts.Orders;
using Production.Application.Features.Orders.Queries.GetQuotesWithOrders;

namespace Production.Infrastructure.Contracts.Orders;

/// <summary>Production orders of a КП, read through the Production query (R-33).</summary>
internal sealed class QuoteOrderLinks(ISender sender) : IQuoteOrderLinks
{
    public async Task<IReadOnlySet<long>> QuotesWithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotesWithOrdersQuery(quoteIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Production orders could not be read.");
        return result.Value.ToHashSet();
    }
}
