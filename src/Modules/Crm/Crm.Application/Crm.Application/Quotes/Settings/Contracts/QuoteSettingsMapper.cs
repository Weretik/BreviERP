using Crm.Domain.Quotes;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Quotes.Settings;

namespace Crm.Application.Quotes.Settings.Contracts;

public static class QuoteSettingsMapper
{
    public static QuoteSettingsResponse ToResponse(this QuoteSettings? saved, IReadOnlyList<QuoteStaffMember> staff,
        IReadOnlyDictionary<int, string> mailboxes)
    {
        var content = saved?.Content ?? QuoteSettingsDefaults.Content;
        return new QuoteSettingsResponse(
            saved?.Version ?? 0,
            saved?.UpdatedByName,
            saved?.UpdatedAt,
            ToDto(content.Company),
            new QuoteEmailTemplateDto(content.Email.Subject, content.Email.Body),
            ToDto(content.Pdf),
            ToDto(content.Introduction),
            content.LetterVariants.Select(x => new QuoteLetterVariantDto(x.Name, x.Subject, x.Body)).ToArray(),
            staff.Select(member => ToResponse(member, saved?.ContactOf(member.UserId), mailboxes.GetValueOrDefault(member.UserId)))
                .ToArray());
    }

    /// <summary>The contact a client sees: the saved one; its empty phone or email, or a missing contact, use the account.</summary>
    public static QuoteStaffContact ContactOrAccount(QuoteStaffMember member, QuoteStaffContact? saved) =>
        new(member.UserId,
            saved?.Name ?? member.Name,
            saved?.Phone ?? member.Phone,
            saved?.Email ?? member.Email);

    public static QuoteSettingsContent ToContent(this SaveQuoteSettingsInput input) =>
        new(
            new QuoteCompanyDetails(input.Company.Name.Trim(), Text(input.Company.Edrpou), Text(input.Company.TaxId),
                Text(input.Company.Iban), Text(input.Company.Address), Text(input.Company.Phone),
                Text(input.Company.Email), Text(input.Company.Site), Text(input.Company.LogoUrl)),
            new QuoteEmailTemplate(input.Email.Subject.Trim(), input.Email.Body.Trim()),
            new QuotePdfTexts(input.Pdf.Title.Trim(), Text(input.Pdf.VatNotice), Text(input.Pdf.CtaTitle),
                Text(input.Pdf.CtaText), Text(input.Pdf.CtaContactTemplate)),
            new QuoteIntroductionTexts(
                Text(input.Introduction.LogoText), Text(input.Introduction.SummerTitle),
                Text(input.Introduction.WinterTitle), Text(input.Introduction.Subtitle),
                Text(input.Introduction.SummerBannerUrl), Text(input.Introduction.WinterBannerUrl),
                Text(input.Introduction.ModelsTitle), Text(input.Introduction.AssortmentLinkText),
                Text(input.Introduction.AssortmentUrl), Text(input.Introduction.ProductButtonText),
                Text(input.Introduction.NextStepText), Text(input.Introduction.AccentColor),
                input.Introduction.Advantages.Select(x => new QuoteAdvantage(x.Title.Trim(), Text(x.Text))).ToArray(),
                Text(input.Introduction.SummerMainCode), Text(input.Introduction.WinterMainCode)),
            input.LetterVariants.Select(x => new QuoteLetterVariant(x.Name.Trim(), x.Subject.Trim(), x.Body.Trim()))
                .ToArray(),
            input.StaffContacts
                .Select(x => new QuoteStaffContact(x.UserId, Text(x.Name) ?? "", Text(x.Phone), Text(x.Email)))
                .ToArray());

    /// <summary>The page edits what was saved; before that it starts from the account.</summary>
    private static QuoteStaffContactResponse ToResponse(QuoteStaffMember member, QuoteStaffContact? saved,
        string? mailbox)
    {
        var contact = saved ?? new QuoteStaffContact(member.UserId, member.Name, member.Phone, member.Email);
        return new QuoteStaffContactResponse(member.UserId, member.Name, member.Email, contact.Name, contact.Phone,
            contact.Email, mailbox);
    }

    private static QuoteCompanyDto ToDto(QuoteCompanyDetails x) =>
        new(x.Name, x.Edrpou, x.TaxId, x.Iban, x.Address, x.Phone, x.Email, x.Site, x.LogoUrl);

    private static QuotePdfTextsDto ToDto(QuotePdfTexts x) =>
        new(x.Title, x.VatNotice, x.CtaTitle, x.CtaText, x.CtaContactTemplate);

    private static QuoteIntroductionTextsDto ToDto(QuoteIntroductionTexts x) =>
        new(x.LogoText, x.SummerTitle, x.WinterTitle, x.Subtitle, x.SummerBannerUrl, x.WinterBannerUrl, x.ModelsTitle,
            x.AssortmentLinkText, x.AssortmentUrl, x.ProductButtonText, x.NextStepText, x.AccentColor,
            x.Advantages.Select(a => new QuoteAdvantageDto(a.Title, a.Text)).ToArray(),
            x.MainCode(QuoteIntroSeason.Summer), x.MainCode(QuoteIntroSeason.Winter));

    private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
