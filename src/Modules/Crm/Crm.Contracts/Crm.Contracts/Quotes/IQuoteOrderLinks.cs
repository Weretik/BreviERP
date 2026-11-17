namespace Crm.Contracts.Quotes;

/// <summary>
/// Which КП already have a production order, for the КП list (R-33). Crm declares it and Production implements it, so
/// Crm does not depend on Production (ADR-0019).
/// </summary>
public interface IQuoteOrderLinks
{
    Task<IReadOnlySet<long>> QuotesWithOrdersAsync(IReadOnlyCollection<long> quoteIds,
        CancellationToken cancellationToken);
}
