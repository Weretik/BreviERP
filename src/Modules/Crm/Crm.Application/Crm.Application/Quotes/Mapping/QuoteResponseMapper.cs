using Crm.Application.Quotes.Contracts;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Mapping;

internal static class QuoteResponseMapper
{
    public static QuoteResponse ToResponse(this Quote quote) => new(
        quote.Id, quote.Number, quote.Kind.ToString(), quote.Status.ToString(), quote.InternalName, quote.CustomerId,
        quote.RecipientName, quote.CompanyName, quote.ClientComment, quote.ManagerNote,
        new QuotePersonResponse(quote.AuthorUserId, quote.AuthorName),
        new QuotePersonResponse(quote.ResponsibleUserId, quote.ResponsibleName),
        quote.CreatedAt, quote.UpdatedAt, quote.LastSentAt, quote.ArchivedAt, quote.ArchivedByName, quote.Version,
        quote.Items.OrderBy(x => x.Position).Select(ToResponse).ToArray(),
        quote.IntroLayout is { } layout
            ? new QuoteIntroductionResponse(layout.ToString(), quote.RecipientEmail, quote.LetterName)
            : null);

    private static QuoteItemResponse ToResponse(QuoteItem item) => new(
        item.Id, item.Kind.ToString(), item.ProductId, item.Code, item.Name, item.Subtitle, item.Description,
        item.SiteUrl, item.PhotoUrl, item.Season, item.SupplierUrl,
        item.Options.OrderBy(x => x.Position).Select(ToResponse).ToArray(),
        item.Lines.OrderBy(x => x.Position).Select(x => new QuoteItemLineResponse(x.Id, x.Kind.ToString(),
            x.ReferenceId, x.Name, x.Unit, x.Quantity, x.ManualMinutes, x.CostCode)).ToArray(),
        item.EmbroideryCalculationId);

    private static QuoteOptionResponse ToResponse(QuoteItemOption option) => new(
        option.Id, option.ArticleId, option.FabricId, option.Title, option.Details, option.IsRecommended,
        ToResponse(option.Retail), ToResponse(option.Wholesale), ToResponse(option.Large),
        option.ManualReason, option.ManualByName, option.ManualAt, option.PricesTakenAt, option.FabricMeters,
        option.Fabrics.OrderBy(x => x.Position).Select(x => new QuoteOptionFabricResponse(x.FabricId, x.Name, x.Meters))
            .ToArray());

    private static QuotePriceResponse ToResponse(QuotePrice price) =>
        new(price.Amount, price.Source, price.IsManual, price.Shown);

    public static QuoteTierPricesResponse Sources(this QuoteItemOption option) =>
        new(option.Retail.Source, option.Wholesale.Source, option.Large.Source);
}
