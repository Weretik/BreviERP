namespace Crm.Application.Quotes.Settings.Contracts;

public sealed record QuoteCompanyDto(
    string Name,
    string? Edrpou,
    string? TaxId,
    string? Iban,
    string? Address,
    string? Phone,
    string? Email,
    string? Site,
    string? LogoUrl);

/// <param name="Body">Marks: {quoteNumber}, {managerName}, {managerPhone}, {managerEmail}, {site}, {clientName}, {companyName}.</param>
public sealed record QuoteEmailTemplateDto(string Subject, string Body);

/// <param name="CtaContactTemplate">Marks: {managerName}, {managerPhone}, {managerEmail}, {clientName}, {companyName}.</param>
/// <param name="Intros">«Вступний текст у КП» (R-35); the response fills empty ones with the standard text.</param>
/// <param name="Labels">Small PDF texts (R-35); the response fills empty ones with the standard text.</param>
public sealed record QuotePdfTextsDto(
    string Title,
    string? VatNotice,
    string? CtaTitle,
    string? CtaText,
    string? CtaContactTemplate,
    QuotePdfIntroTextsDto? Intros = null,
    QuotePdfLabelsDto? Labels = null);

/// <summary>Marks: {recipient}, {sewingModel}, {fabricName}, {ppeItem} and the manager's.</summary>
public sealed record QuotePdfIntroTextsDto(
    string? OnePpe,
    string? OneSewing,
    string? OneSewingOnePpe,
    string? Sewing,
    string? Ppe,
    string? SewingAndPpe,
    string? Fallback);

/// <param name="ContactLine">Marks: {managerName}, {managerPhone}, {managerEmail}, {clientName}, {companyName}.</param>
/// <param name="ClientTitle">The small heading over the client in the PDF («Для»).</param>
/// <param name="ShowClient">False leaves the client block («Для» with the company and the recipient) out of the PDF.</param>
public sealed record QuotePdfLabelsDto(
    string? ContactLine,
    string? CodeLabel,
    string? SiteLink,
    string? FabricsTitle,
    string? Recommended,
    string? CommentTitle,
    string? ClientTitle = null,
    bool? ShowClient = null);

public sealed record QuoteAdvantageDto(string Title, string? Text);

public sealed record QuoteIntroductionTextsDto(
    string? LogoText,
    string? SummerTitle,
    string? WinterTitle,
    string? Subtitle,
    string? SummerBannerUrl,
    string? WinterBannerUrl,
    string? ModelsTitle,
    string? AssortmentLinkText,
    string? AssortmentUrl,
    string? ProductButtonText,
    string? NextStepText,
    string? AccentColor,
    IReadOnlyList<QuoteAdvantageDto> Advantages,
    string? SummerMainCode = null,
    string? WinterMainCode = null,
    string? PricesNote = null);

public sealed record QuoteLetterVariantDto(string Name, string Subject, string Body);

/// <summary>A staff member's contact in a КП; empty fields fall back to the account.</summary>
public sealed record QuoteStaffContactInput(int UserId, string? Name, string? Phone, string? Email);

/// <param name="AccountName">The account's name and email, so the page shows whose contact it is.</param>
/// <param name="MailboxEmail">The Gmail КП are sent from; null until connected (R-15).</param>
public sealed record QuoteStaffContactResponse(
    int UserId,
    string AccountName,
    string? AccountEmail,
    string Name,
    string? Phone,
    string? Email,
    string? MailboxEmail);

/// <param name="Version">0 before the first save; send it back to save.</param>
public sealed record QuoteSettingsResponse(
    int Version,
    string? UpdatedByName,
    DateTimeOffset? UpdatedAt,
    QuoteCompanyDto Company,
    QuoteEmailTemplateDto Email,
    QuotePdfTextsDto Pdf,
    QuoteIntroductionTextsDto Introduction,
    IReadOnlyList<QuoteLetterVariantDto> LetterVariants,
    IReadOnlyList<QuoteStaffContactResponse> StaffContacts);

public sealed record SaveQuoteSettingsInput(
    int Version,
    QuoteCompanyDto Company,
    QuoteEmailTemplateDto Email,
    QuotePdfTextsDto Pdf,
    QuoteIntroductionTextsDto Introduction,
    IReadOnlyList<QuoteLetterVariantDto> LetterVariants,
    IReadOnlyList<QuoteStaffContactInput> StaffContacts);
