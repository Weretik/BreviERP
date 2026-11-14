namespace Crm.Contracts.Quotes;

/// <summary>Who reads: the user and whether they may see every manager's КП.</summary>
public sealed record QuoteReader(int UserId, bool CanSeeAll);

/// <param name="Kind">Request («під запит») or Introduction («для знайомства»).</param>
public sealed record QuoteSummaryInfo(long Id, string Number, string Kind, string? CompanyName, string InternalName,
    string ResponsibleName, DateTimeOffset? LastSentAt);

public sealed record QuoteInfo(long Id, string Number, string Kind, string? CompanyName, string InternalName,
    long? CustomerId, int ResponsibleUserId, string ResponsibleName, IReadOnlyList<QuoteItemInfo> Items);

/// <param name="Kind">Sewing, CustomSewing, Ppe, Embroidery or another item kind.</param>
public sealed record QuoteItemInfo(long Id, string Kind, int? ProductId, string? Code, string Name,
    long? EmbroideryCalculationId, IReadOnlyList<QuoteOptionInfo> Options);

/// <summary>A variant of an item with its prices by tier as the client sees them.</summary>
public sealed record QuoteOptionInfo(long Id, long? ArticleId, int? FabricId, string Title, decimal? Retail,
    decimal? Wholesale, decimal? Large);

/// <summary>КП of Crm for other modules, under the reader's access rule.</summary>
public interface IQuoteReader
{
    /// <summary>The latest КП, up to <paramref name="max"/>, matching <paramref name="search"/>.</summary>
    Task<IReadOnlyList<QuoteSummaryInfo>> ListAsync(QuoteReader reader, string? search, int max,
        CancellationToken cancellationToken);

    Task<QuoteInfo?> FindAsync(QuoteReader reader, long quoteId, CancellationToken cancellationToken);
}
