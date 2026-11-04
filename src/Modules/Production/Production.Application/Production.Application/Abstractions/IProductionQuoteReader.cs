namespace Production.Application.Abstractions;

/// <summary>
/// Sent КП of Crm for «Передати в цех»; implemented in the host over the Crm quote queries. A Manager reads only the
/// КП they are responsible for; Admin and Director read all.
/// </summary>
public interface IProductionQuoteReader
{
    Task<IReadOnlyList<ProductionQuoteSummary>> ListSentAsync(ProductionQuoteReaderActor actor, string? search,
        CancellationToken cancellationToken);

    /// <returns>Null when the КП is missing or the person may not see it.</returns>
    Task<ProductionQuote?> ReadAsync(ProductionQuoteReaderActor actor, long quoteId, CancellationToken cancellationToken);
}

public sealed record ProductionQuoteReaderActor(int UserId, bool CanSeeAll);

public sealed record ProductionQuoteSummary(
    long Id, string Number, string ClientName, string ResponsibleName, DateTimeOffset? LastSentAt);

/// <param name="IsSent">Only a sent КП goes to the workshop.</param>
public sealed record ProductionQuote(
    long Id, string Number, bool IsSent, bool IsArchived, string ClientName, long? CustomerId, int ResponsibleUserId,
    string ResponsibleName, IReadOnlyList<ProductionQuoteItem> Items);

/// <param name="ProductId">The Catalog model; null for a «Виріб з нуля».</param>
public sealed record ProductionQuoteItem(
    long Id, int? ProductId, string? Code, string Name, IReadOnlyList<ProductionQuoteOption> Options);

public sealed record ProductionQuoteOption(
    long Id, long? ArticleId, int? FabricId, string Title, decimal? Retail, decimal? Wholesale, decimal? Large);
