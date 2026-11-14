using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Introductions.Building;
using Crm.Application.Features.Quotes.Shared.Pdf;

namespace Crm.Application.Features.Quotes.Shared.Pdf.Introduction;

/// <summary>Builds the client view of a saved «КП для знайомства» for the PDF (R-16).</summary>
public static class QuoteIntroPdfModelBuilder
{
    public const string DefaultAccent = "#FF6500";
    private const string DefaultButton = "Детальніше";
    private const string DensityMark = "щільність ";

    /// <param name="catalogPhotos">Main photos of the products whose items have none (R-29).</param>
    public static QuoteIntroPdfModel Build(Quote quote, QuoteSettingsContent settings, QuoteStaffContact manager,
        IReadOnlyDictionary<int, string>? catalogPhotos = null)
    {
        var photos = catalogPhotos ?? new Dictionary<int, string>();
        var introduction = quote.Introduction
            ?? throw new InvalidOperationException("Only a «КП для знайомства» has catalogue pages.");
        var texts = settings.Introduction;
        var company = settings.Company;
        var pages = QuoteIntroPages.Split(introduction, quote.Items)
            .Select(page => new QuoteIntroPdfPage(
                page.Season,
                (page.Season == QuoteIntroSeason.Summer ? texts.SummerTitle : texts.WinterTitle)
                    ?? QuoteIntroPages.Title(page.Season),
                texts.Subtitle,
                page.Season == QuoteIntroSeason.Summer ? texts.SummerBannerUrl : texts.WinterBannerUrl,
                texts.ModelsTitle,
                page.Items.Select((item, index) => ToCard(item, index == 0, photos)).ToArray()))
            .ToArray();

        return new QuoteIntroPdfModel(
            quote.Number,
            new QuotePdfCompany(company.Name, company.Edrpou, company.TaxId, company.Iban, company.Address,
                company.Phone, company.Email, company.Site, company.LogoUrl),
            texts.LogoText,
            texts.AccentColor ?? DefaultAccent,
            pages,
            texts.AssortmentLinkText,
            texts.AssortmentUrl ?? company.Site,
            texts.ProductButtonText ?? DefaultButton,
            texts.Advantages.Select(x => new QuoteIntroPdfAdvantage(x.Title, x.Text)).ToArray(),
            texts.PricesNote,
            texts.NextStepText,
            new QuotePdfContact(manager.Name, manager.Phone, manager.Email));
    }

    /// <summary>«60% бавовна, 40% поліестер, щільність 235 г/м²» prints as two lines: composition, then density.</summary>
    public static IReadOnlyList<string> FabricLines(string? details)
    {
        if (string.IsNullOrWhiteSpace(details)) return [];
        var text = details.Trim();
        var at = text.IndexOf(DensityMark, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return [text];
        var composition = text[..at].TrimEnd(' ', ',');
        var density = text[(at + DensityMark.Length)..].Trim();
        return new[] { composition, density }.Where(x => x.Length > 0).ToArray();
    }

    private static QuoteIntroPdfCard ToCard(QuoteItem item, bool isMain, IReadOnlyDictionary<int, string> photos)
    {
        var option = item.Options.OrderBy(x => x.Position).FirstOrDefault();
        return new QuoteIntroPdfCard(item.Name, option?.Title, FabricLines(option?.Details),
            QuoteItemPhotos.Resolve(item.ProductId, item.PhotoUrl, photos), item.SiteUrl,
            option?.Large.IsVisiblePositive == true ? QuotePdfPrices.Up(option.Large.Amount) : null, isMain);
    }
}
