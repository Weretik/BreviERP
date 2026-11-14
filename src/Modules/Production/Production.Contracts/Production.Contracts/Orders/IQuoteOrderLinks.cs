namespace Production.Contracts.Orders;

/// <summary>Which КП already have a production order, for the КП list (R-33).</summary>
public interface IQuoteOrderLinks
{
    Task<IReadOnlySet<long>> QuotesWithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken);
}
