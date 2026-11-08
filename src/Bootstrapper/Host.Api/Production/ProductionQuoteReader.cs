using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.GetQuote;
using Crm.Application.Quotes.GetQuotes;
using Mediator;
using Production.Application.Abstractions;

namespace Host.Api.Production;

/// <summary>Sent КП of Crm for «Передати в цех», read through the Crm quote queries; sewn items and the «Вишивка» rows.</summary>
internal sealed class ProductionQuoteReader(ISender sender) : IProductionQuoteReader
{
    private const int MaxSources = 100;

    public async Task<IReadOnlyList<ProductionQuoteSummary>> ListForWorkshopAsync(ProductionQuoteReaderActor actor,
        string? search, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotesQuery(new QuoteActor(actor.UserId, actor.CanSeeAll),
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(), null, null, 1, MaxSources), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Crm quotes could not be read.");
        return result.Value.Items
            .Where(x => x.Kind == "Request")
            .Select(x => new ProductionQuoteSummary(x.Id, x.Number, x.CompanyName ?? x.InternalName,
                x.Responsible.Name, x.LastSentAt))
            .ToArray();
    }

    public async Task<ProductionQuote?> ReadAsync(ProductionQuoteReaderActor actor, long quoteId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuoteQuery(new QuoteActor(actor.UserId, actor.CanSeeAll), quoteId),
            cancellationToken);
        if (!result.IsSuccess) return null;
        var quote = result.Value;
        return new ProductionQuote(quote.Id, quote.Number, quote.Kind == "Request",
            quote.CompanyName ?? quote.InternalName, quote.CustomerId,
            quote.Responsible.UserId, quote.Responsible.Name,
            quote.Items.Where(IsSewn).Select(Item).ToArray(),
            quote.Items.Where(x => x is { Kind: "Embroidery", EmbroideryCalculationId: not null })
                .Select(x => new ProductionQuoteEmbroidery(x.Id, x.EmbroideryCalculationId!.Value, x.Name)).ToArray());
    }

    private static bool IsSewn(QuoteItemResponse item) => item.Kind is "Sewing" or "CustomSewing";

    private static ProductionQuoteItem Item(QuoteItemResponse item) => new(item.Id,
        item.Kind == "Sewing" ? item.ProductId : null, item.Code, item.Name,
        item.Options.Select(x => new ProductionQuoteOption(x.Id, x.ArticleId, x.FabricId, x.Title, x.Retail.Amount,
            x.Wholesale.Amount, x.Large.Amount)).ToArray());
}
