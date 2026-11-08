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
/// «Текст у PDF» (R-14, R-35): everything the КП PDF prints besides the items and the client comment. The contact
/// template takes the responsible manager's marks. <see cref="Intros"/> and <see cref="Labels"/> are null in settings
/// saved before R-35; an empty text means the standard one.
/// </summary>
public sealed record QuotePdfTexts(
    string Title,
    string? VatNotice,
    string? CtaTitle,
    string? CtaText,
    string? CtaContactTemplate,
    QuotePdfIntroTexts? Intros = null,
    QuotePdfLabels? Labels = null)
{
    /// <summary>The opening text for the КП composition; a missing one is the standard text of that case.</summary>
    public string Intro(QuotePdfIntroCase intro)
    {
        var saved = Intros?.Of(intro);
        return string.IsNullOrWhiteSpace(saved) ? QuoteSettingsDefaults.Intros.Of(intro)! : saved;
    }

    /// <summary>The PDF labels with each missing one taken from the standard ones.</summary>
    public QuotePdfLabels ResolvedLabels() => QuotePdfLabels.Merge(Labels, QuoteSettingsDefaults.Labels);
}

/// <summary>
/// «Вступний текст у КП» (R-35): the system picks one by the КП composition. Marks: {recipient}, {sewingModel},
/// {fabricName}, {ppeItem} and the manager's.
/// </summary>
public sealed record QuotePdfIntroTexts(
    string? OnePpe,
    string? OneSewing,
    string? OneSewingOnePpe,
    string? Sewing,
    string? Ppe,
    string? SewingAndPpe,
    string? Fallback)
{
    public string? Of(QuotePdfIntroCase intro) => intro switch
    {
        QuotePdfIntroCase.OnePpe => OnePpe,
        QuotePdfIntroCase.OneSewing => OneSewing,
        QuotePdfIntroCase.OneSewingOnePpe => OneSewingOnePpe,
        QuotePdfIntroCase.Sewing => Sewing,
        QuotePdfIntroCase.Ppe => Ppe,
        QuotePdfIntroCase.SewingAndPpe => SewingAndPpe,
        _ => Fallback,
    };
}

/// <summary>Which opening text a КП gets (R-35).</summary>
public enum QuotePdfIntroCase
{
    /// <summary>Only one PPE item.</summary>
    OnePpe = 1,

    /// <summary>One sewing model with one fabric variant.</summary>
    OneSewing = 2,

    /// <summary>One sewing model with one fabric variant and one PPE item.</summary>
    OneSewingOnePpe = 3,

    /// <summary>Several sewing models or several fabric variants, no PPE.</summary>
    Sewing = 4,

    /// <summary>Several PPE items, no sewing.</summary>
    Ppe = 5,

    /// <summary>Several items, both sewing and PPE.</summary>
    SewingAndPpe = 6,

    /// <summary>None of the above, for example only embroidery.</summary>
    Fallback = 7,
}

/// <summary>
/// The small texts of the КП PDF (R-35). <see cref="ContactLine"/> goes under the price note and takes the manager's
/// marks.
/// </summary>
public sealed record QuotePdfLabels(
    string? ContactLine,
    string? CodeLabel,
    string? SiteLink,
    string? FabricsTitle,
    string? Recommended,
    string? CommentTitle)
{
    public static QuotePdfLabels Merge(QuotePdfLabels? saved, QuotePdfLabels standard) => new(
        Pick(saved?.ContactLine, standard.ContactLine),
        Pick(saved?.CodeLabel, standard.CodeLabel),
        Pick(saved?.SiteLink, standard.SiteLink),
        Pick(saved?.FabricsTitle, standard.FabricsTitle),
        Pick(saved?.Recommended, standard.Recommended),
        Pick(saved?.CommentTitle, standard.CommentTitle));

    private static string? Pick(string? saved, string? standard) => string.IsNullOrWhiteSpace(saved) ? standard : saved;
}

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
