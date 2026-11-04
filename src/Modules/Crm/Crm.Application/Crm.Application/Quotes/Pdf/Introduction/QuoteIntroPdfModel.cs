namespace Crm.Application.Quotes.Pdf.Introduction;

/// <summary>
/// The client view of a «КП для знайомства» (R-16, R-14): season pages with 4 models each, the advantages, the next
/// step and the manager's contact. Internal names, notes and hidden prices are not part of it.
/// </summary>
public sealed record QuoteIntroPdfModel(
    string Number,
    DateOnly Date,
    QuotePdfCompany Company,
    string? LogoText,
    string Accent,
    string? RecipientName,
    string? ClientCompanyName,
    IReadOnlyList<QuoteIntroPdfPage> Pages,
    string? AssortmentLinkText,
    string? AssortmentUrl,
    string ProductButtonText,
    IReadOnlyList<QuoteIntroPdfAdvantage> Advantages,
    string? NextStepText,
    string? VatNotice,
    QuotePdfContact Manager);

public sealed record QuoteIntroPdfPage(
    string Title,
    string? Subtitle,
    string? BannerUrl,
    string? ModelsTitle,
    IReadOnlyList<QuoteIntroPdfCard> Models);

/// <summary>One model with its fabric; <paramref name="Price"/> is «від 41 шт.» and null when it is hidden.</summary>
public sealed record QuoteIntroPdfCard(
    string Name,
    string? Fabric,
    string? PhotoUrl,
    string? SiteUrl,
    decimal? Price,
    bool IsMain);

public sealed record QuoteIntroPdfAdvantage(string Title, string? Text);
