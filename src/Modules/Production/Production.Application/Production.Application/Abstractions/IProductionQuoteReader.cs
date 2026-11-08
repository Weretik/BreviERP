namespace Production.Application.Abstractions;

/// <summary>
/// КП «під запит» of Crm for «Передати в цех» (saved or sent: managers often send the PDF themselves); implemented in the host over the Crm quote queries. A Manager reads only the
/// КП they are responsible for; Admin and Director read all.
/// </summary>
public interface IProductionQuoteReader
{
    Task<IReadOnlyList<ProductionQuoteSummary>> ListForWorkshopAsync(ProductionQuoteReaderActor actor, string? search,
        CancellationToken cancellationToken);

    /// <returns>Null when the КП is missing or the person may not see it.</returns>
    Task<ProductionQuote?> ReadAsync(ProductionQuoteReaderActor actor, long quoteId, CancellationToken cancellationToken);
}

public sealed record ProductionQuoteReaderActor(int UserId, bool CanSeeAll);

public sealed record ProductionQuoteSummary(
    long Id, string Number, string ClientName, string ResponsibleName, DateTimeOffset? LastSentAt);

/// <param name="ForWorkshop">Only a КП «під запит» goes to the workshop; «для знайомства» does not.</param>
public sealed record ProductionQuote(
    long Id, string Number, bool ForWorkshop, string ClientName, long? CustomerId, int ResponsibleUserId,
    string ResponsibleName, IReadOnlyList<ProductionQuoteItem> Items,
    IReadOnlyList<ProductionQuoteEmbroidery>? Embroideries = null);

/// <summary>An «Вишивка» row of the КП: the saved calculation it was added from.</summary>
public sealed record ProductionQuoteEmbroidery(long QuoteItemId, long CalculationId, string Name);

/// <param name="ProductId">The Catalog model; null for a «Виріб з нуля».</param>
public sealed record ProductionQuoteItem(
    long Id, int? ProductId, string? Code, string Name, IReadOnlyList<ProductionQuoteOption> Options);

public sealed record ProductionQuoteOption(
    long Id, long? ArticleId, int? FabricId, string Title, decimal? Retail, decimal? Wholesale, decimal? Large);
