using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Pdf;

namespace Crm.Application.Features.Quotes.Shared.Pdf.Introduction;

/// <summary>
/// The client view of a «КП для знайомства» (R-16, R-14): one page per season with the banner, the main model,
/// the advantages, three more models, the next step and the manager's contact. Internal names, notes and hidden
/// prices are not part of it.
/// </summary>
public sealed record QuoteIntroPdfModel(
    string Number,
    QuotePdfCompany Company,
    string? LogoText,
    string Accent,
    IReadOnlyList<QuoteIntroPdfPage> Pages,
    string? AssortmentLinkText,
    string? AssortmentUrl,
    string ProductButtonText,
    IReadOnlyList<QuoteIntroPdfAdvantage> Advantages,
    string? PricesNote,
    string? NextStepText,
    QuotePdfContact Manager);

/// <param name="Title">The banner title; a word in *asterisks* is printed in the accent colour.</param>
/// <param name="BannerUrl">The season banner of the settings; null prints the built-in one.</param>
/// <param name="Models">The main model first (on the banner), then the popular models.</param>
public sealed record QuoteIntroPdfPage(
    QuoteIntroSeason Season,
    string Title,
    string? Subtitle,
    string? BannerUrl,
    string? ModelsTitle,
    IReadOnlyList<QuoteIntroPdfCard> Models);

/// <summary>
/// One model with its fabric; <paramref name="Price"/> is «від 41 шт.» rounded up to whole hryvnias (R-35) and null
/// when it is hidden. <paramref name="FabricLines"/> are the composition and density under the fabric name.
/// </summary>
public sealed record QuoteIntroPdfCard(
    string Name,
    string? Fabric,
    IReadOnlyList<string> FabricLines,
    string? PhotoUrl,
    string? SiteUrl,
    decimal? Price,
    bool IsMain);

public sealed record QuoteIntroPdfAdvantage(string Title, string? Text);

/// <summary>A piece of the banner title: <see cref="IsAccent"/> pieces were marked with *asterisks*.</summary>
public sealed record QuoteIntroTitlePart(string Text, bool IsAccent)
{
    public static IReadOnlyList<QuoteIntroTitlePart> Split(string title) =>
        title.Split('*').Select((text, index) => new QuoteIntroTitlePart(text, index % 2 == 1))
            .Where(x => x.Text.Length > 0).ToArray();
}
