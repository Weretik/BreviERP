using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Introductions.Building;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Pdf.Introduction;

/// <summary>Builds the client view of a saved «КП для знайомства» for the PDF (R-16).</summary>
public static class QuoteIntroPdfModelBuilder
{
    public const string DefaultAccent = "#E8780F";
    private const string DefaultButton = "Детальніше";

    public static QuoteIntroPdfModel Build(Quote quote, QuoteSettingsContent settings, QuoteStaffContact manager)
    {
        var introduction = quote.Introduction
            ?? throw new InvalidOperationException("Only a «КП для знайомства» has catalogue pages.");
        var texts = settings.Introduction;
        var company = settings.Company;
        var pages = QuoteIntroPages.Split(introduction, quote.Items)
            .Select(page => new QuoteIntroPdfPage(
                (page.Season == QuoteIntroSeason.Summer ? texts.SummerTitle : texts.WinterTitle)
                    ?? QuoteIntroPages.Title(page.Season),
                texts.Subtitle,
                page.Season == QuoteIntroSeason.Summer ? texts.SummerBannerUrl : texts.WinterBannerUrl,
                texts.ModelsTitle,
                page.Items.Select((item, index) => ToCard(item, index == 0)).ToArray()))
            .ToArray();

        return new QuoteIntroPdfModel(
            quote.Number,
            DateOnly.FromDateTime(QuoteClock.KyivDate(quote.UpdatedAt)),
            new QuotePdfCompany(company.Name, company.Edrpou, company.TaxId, company.Iban, company.Address,
                company.Phone, company.Email, company.Site, company.LogoUrl),
            texts.LogoText,
            texts.AccentColor ?? DefaultAccent,
            quote.RecipientName,
            quote.CompanyName,
            pages,
            texts.AssortmentLinkText,
            texts.AssortmentUrl ?? company.Site,
            texts.ProductButtonText ?? DefaultButton,
            texts.Advantages.Select(x => new QuoteIntroPdfAdvantage(x.Title, x.Text)).ToArray(),
            texts.NextStepText,
            settings.Pdf.VatNotice,
            new QuotePdfContact(manager.Name, manager.Phone, manager.Email));
    }

    private static QuoteIntroPdfCard ToCard(QuoteItem item, bool isMain)
    {
        var option = item.Options.OrderBy(x => x.Position).FirstOrDefault();
        return new QuoteIntroPdfCard(item.Name, option?.Title, item.PhotoUrl, item.SiteUrl,
            option?.Large.IsVisiblePositive == true ? option.Large.Amount : null, isMain);
    }
}
