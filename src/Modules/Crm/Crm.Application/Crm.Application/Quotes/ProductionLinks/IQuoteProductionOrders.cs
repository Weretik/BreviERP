namespace Crm.Application.Quotes.ProductionLinks;

/// <summary>Production orders made from a КП; the Host reads them from the Production module (R-33).</summary>
public interface IQuoteProductionOrders
{
    /// <summary>The КП of <paramref name="quoteIds"/> that have at least one production order that is not deleted.</summary>
    Task<IReadOnlySet<long>> WithOrdersAsync(IReadOnlyCollection<long> quoteIds, CancellationToken cancellationToken);
}
