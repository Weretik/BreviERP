using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Catalog;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Pdf;

/// <summary>Builds the client view of a saved КП for the PDF (R-14).</summary>
public static class QuotePdfModelBuilder
{
    /// <param name="catalogPhotos">Main photos of the products whose items have none (R-29).</param>
    public static QuotePdfModel Build(Quote quote, QuoteSettingsContent settings, QuoteStaffContact manager,
        IReadOnlyDictionary<int, string>? catalogPhotos = null)
    {
        var photos = catalogPhotos ?? new Dictionary<int, string>();
        var items = quote.Items.OrderBy(x => x.Position)
            .Select(x => ToItem(x, photos))
            .Where(x => x.Options.Count > 0)
            .ToArray();
        var options = items.SelectMany(x => x.Options).ToArray();
        var contact = new QuotePdfContact(manager.Name, manager.Phone, manager.Email);
        var intro = QuotePdfText.FillClient(
            string.IsNullOrWhiteSpace(settings.Pdf.IntroText) ? QuoteSettingsDefaults.IntroText : settings.Pdf.IntroText,
            quote.CompanyName ?? quote.RecipientName);

        return new QuotePdfModel(
            quote.Number,
            DateOnly.FromDateTime(QuoteClock.KyivDate(quote.UpdatedAt)),
            settings.Pdf.Title,
            new QuotePdfCompany(settings.Company.Name, settings.Company.Edrpou, settings.Company.TaxId,
                settings.Company.Iban, settings.Company.Address, settings.Company.Phone, settings.Company.Email,
                settings.Company.Site, settings.Company.LogoUrl),
            quote.RecipientName,
            quote.CompanyName,
            FillMarks(intro, contact, settings.Company.Site, quote.Number),
            items,
            new QuotePdfTiers(
                options.Any(x => x.Retail is not null),
                options.Any(x => x.Wholesale is not null),
                options.Any(x => x.Large is not null)),
            quote.ClientComment,
            settings.Pdf.VatNotice,
            settings.Pdf.CtaTitle,
            settings.Pdf.CtaText,
            FillMarks(settings.Pdf.CtaContactTemplate, contact, settings.Company.Site, quote.Number),
            contact);
    }

    /// <summary>Replaces {managerName}, {managerPhone}, {managerEmail}, {site} and {quoteNumber}.</summary>
    public static string? FillMarks(string? template, QuotePdfContact manager, string? site, string quoteNumber) =>
        template?
            .Replace("{managerName}", manager.Name, StringComparison.Ordinal)
            .Replace("{managerPhone}", manager.Phone ?? "", StringComparison.Ordinal)
            .Replace("{managerEmail}", manager.Email ?? "", StringComparison.Ordinal)
            .Replace("{site}", site ?? "", StringComparison.Ordinal)
            .Replace("{quoteNumber}", quoteNumber, StringComparison.Ordinal);

    private static QuotePdfItem ToItem(QuoteItem item, IReadOnlyDictionary<int, string> photos)
    {
        var options = item.Options.OrderBy(x => x.Position)
            .Where(x => x.ShowsPositivePrice)
            .Select(x => new QuotePdfOption(x.Title, x.Details, x.IsRecommended, Visible(x.Retail),
                Visible(x.Wholesale), Visible(x.Large)))
            .ToArray();
        return new QuotePdfItem(item.Name, item.Code, item.Subtitle, item.Season, QuotePdfText.Unwrap(item.Description),
            QuoteItemPhotos.Resolve(item.ProductId, item.PhotoUrl, photos), item.SiteUrl,
            item.Kind is QuoteItemKind.Sewing or QuoteItemKind.CustomSewing ? "Варіант тканини" : "Ціна за одиницю",
            options, QuotePdfPrices.Columns(options));
    }

    private static decimal? Visible(QuotePrice price) => price.IsVisiblePositive ? price.Amount : null;
}
