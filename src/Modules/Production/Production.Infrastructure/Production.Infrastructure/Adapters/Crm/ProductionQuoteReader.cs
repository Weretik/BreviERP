using Crm.Contracts.Quotes;
using Production.Application.Abstractions;

namespace Production.Infrastructure.Adapters.Crm;

/// <summary>Sent КП of Crm for «Передати в цех»: sewn items and the «Вишивка» rows.</summary>
internal sealed class ProductionQuoteReader(IQuoteReader quotes) : IProductionQuoteReader
{
    private const int MaxSources = 100;

    public async Task<IReadOnlyList<ProductionQuoteSummary>> ListForWorkshopAsync(ProductionQuoteReaderActor actor,
        string? search, CancellationToken cancellationToken) =>
        (await quotes.ListAsync(new QuoteReader(actor.UserId, actor.CanSeeAll),
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(), MaxSources, cancellationToken))
        .Where(x => x.Kind == "Request")
        .Select(x => new ProductionQuoteSummary(x.Id, x.Number, x.CompanyName ?? x.InternalName, x.ResponsibleName,
            x.LastSentAt))
        .ToArray();

    public async Task<ProductionQuote?> ReadAsync(ProductionQuoteReaderActor actor, long quoteId,
        CancellationToken cancellationToken)
    {
        var quote = await quotes.FindAsync(new QuoteReader(actor.UserId, actor.CanSeeAll), quoteId, cancellationToken);
        if (quote is null) return null;
        return new ProductionQuote(quote.Id, quote.Number, quote.Kind == "Request",
            quote.CompanyName ?? quote.InternalName, quote.CustomerId, quote.ResponsibleUserId, quote.ResponsibleName,
            quote.Items.Where(IsSewn).Select(Item).ToArray(),
            quote.Items.Where(x => x is { Kind: "Embroidery", EmbroideryCalculationId: not null })
                .Select(x => new ProductionQuoteEmbroidery(x.Id, x.EmbroideryCalculationId!.Value, x.Name)).ToArray());
    }

    private static bool IsSewn(QuoteItemInfo item) => item.Kind is "Sewing" or "CustomSewing";

    private static ProductionQuoteItem Item(QuoteItemInfo item) => new(item.Id,
        item.Kind == "Sewing" ? item.ProductId : null, item.Code, item.Name,
        item.Options.Select(x => new ProductionQuoteOption(x.Id, x.ArticleId, x.FabricId, x.Title, x.Retail,
            x.Wholesale, x.Large)).ToArray());
}
