namespace Crm.Domain.Quotes.Settings;

/// <summary>«Реквізити та контакти Brevi»: printed in the PDF header and footer.</summary>
public sealed record QuoteCompanyDetails(
    string Name,
    string? Edrpou,
    string? TaxId,
    string? Iban,
    string? Address,
    string? Phone,
    string? Email,
    string? Site,
    string? LogoUrl);

/// <summary>The letter that goes with a КП (stage 3); marks in braces are filled when sending.</summary>
public sealed record QuoteEmailTemplate(string Subject, string Body);

/// <summary>
/// Texts of the КП PDF; the contact template takes the responsible manager's marks. <see cref="IntroText"/> opens
/// the КП under the client's name and takes {client} as well; empty means <see cref="QuoteSettingsDefaults.IntroText"/>.
/// </summary>
public sealed record QuotePdfTexts(
    string Title,
    string? VatNotice,
    string? CtaTitle,
    string? CtaText,
    string? CtaContactTemplate,
    string? IntroText = null);

/// <summary>One of the five «Переваги» of the introduction catalogue.</summary>
public sealed record QuoteAdvantage(string Title, string? Text);

/// <summary>
/// «КП для знайомства — рекламний каталог» texts and the product codes of the fixed main models of the summer and
/// winter pages (R-16); a missing code means the default one.
/// </summary>
public sealed record QuoteIntroductionTexts(
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
    IReadOnlyList<QuoteAdvantage> Advantages,
    string? SummerMainCode = null,
    string? WinterMainCode = null)
{
    public const string DefaultSummerMainCode = "MOD-166";
    public const string DefaultWinterMainCode = "MOD-169";

    public string MainCode(QuoteIntroSeason season) => season == QuoteIntroSeason.Summer
        ? SummerMainCode ?? DefaultSummerMainCode
        : WinterMainCode ?? DefaultWinterMainCode;
}

/// <summary>A letter the manager picks before sending an introduction КП.</summary>
public sealed record QuoteLetterVariant(string Name, string Subject, string Body);

/// <summary>What a staff member shows the client as their contact in a КП.</summary>
public sealed record QuoteStaffContact(int UserId, string Name, string? Phone, string? Email);
