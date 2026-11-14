using Crm.Contracts.Quotes;
using Mediator;
using Crm.Application.Features.Quotes.Queries.GetQuote;
using Crm.Application.Features.Quotes.Queries.GetQuotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Infrastructure.Contracts.Quotes;

/// <summary>КП for other modules through the Crm quote queries, under the reader's access rule.</summary>
internal sealed class QuoteDirectory(ISender sender) : IQuoteReader
{
    public async Task<IReadOnlyList<QuoteSummaryInfo>> ListAsync(QuoteReader reader,
        string? search, int max, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotesQuery(Actor(reader), search, null, null, 1, max),
            cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Crm quotes could not be read.");
        return result.Value.Items.Select(x => new QuoteSummaryInfo(x.Id, x.Number, x.Kind, x.CompanyName,
            x.InternalName, x.Responsible.Name, x.LastSentAt)).ToArray();
    }

    public async Task<QuoteInfo?> FindAsync(QuoteReader reader, long quoteId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuoteQuery(Actor(reader), quoteId), cancellationToken);
        if (!result.IsSuccess) return null;
        var quote = result.Value;
        return new QuoteInfo(quote.Id, quote.Number, quote.Kind, quote.CompanyName, quote.InternalName,
            quote.CustomerId, quote.Responsible.UserId, quote.Responsible.Name, quote.Items.Select(Item).ToArray());
    }

    private static QuoteActor Actor(QuoteReader reader) =>
        new(reader.UserId, reader.CanSeeAll);

    private static QuoteItemInfo Item(QuoteItemResponse item) => new(item.Id, item.Kind, item.ProductId, item.Code,
        item.Name, item.EmbroideryCalculationId,
        item.Options.Select(x => new QuoteOptionInfo(x.Id, x.ArticleId, x.FabricId, x.Title, x.Retail.Amount,
            x.Wholesale.Amount, x.Large.Amount)).ToArray());
}
