using Crm.Domain.Quotes;
using Crm.Application.Quotes.Settings.Contracts;
using L = Crm.Domain.Quotes.Settings.QuoteSettingsLimits;

namespace Crm.Application.Quotes.Settings.SaveSettings;

public sealed class SaveQuoteSettingsCommandValidator : AbstractValidator<SaveQuoteSettingsCommand>
{
    public SaveQuoteSettingsCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteSettingsInputValidator());
    }
}

public sealed class SaveQuoteSettingsInputValidator : AbstractValidator<SaveQuoteSettingsInput>
{
    public SaveQuoteSettingsInputValidator()
    {
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Company).NotNull().SetValidator(new QuoteCompanyValidator());
        RuleFor(x => x.Email).NotNull().ChildRules(email =>
        {
            email.RuleFor(x => x.Subject).NotEmpty().WithMessage("Вкажіть тему листа.").MaximumLength(L.ShortTextMaxLength);
            email.RuleFor(x => x.Body).NotEmpty().WithMessage("Вкажіть текст листа.").MaximumLength(L.TextMaxLength);
        });
        RuleFor(x => x.Pdf).NotNull().ChildRules(pdf =>
        {
            pdf.RuleFor(x => x.Title).NotEmpty().WithMessage("Вкажіть заголовок PDF.").MaximumLength(L.ShortTextMaxLength);
            pdf.RuleFor(x => x.VatNotice).MaximumLength(L.ShortTextMaxLength);
            pdf.RuleFor(x => x.CtaTitle).MaximumLength(L.ShortTextMaxLength);
            pdf.RuleFor(x => x.CtaText).MaximumLength(L.TextMaxLength);
            pdf.RuleFor(x => x.CtaContactTemplate).MaximumLength(L.TextMaxLength);
            pdf.RuleFor(x => x.IntroText).MaximumLength(L.TextMaxLength);
        });
        RuleFor(x => x.Introduction).NotNull().SetValidator(new QuoteIntroductionValidator());
        RuleFor(x => x.LetterVariants).NotNull()
            .Must(x => x is { Count: <= L.MaxLetterVariants })
            .WithMessage($"Не більше {L.MaxLetterVariants} варіантів листа.");
        RuleForEach(x => x.LetterVariants).NotNull().ChildRules(letter =>
        {
            letter.RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву варіанта листа.").MaximumLength(L.ShortTextMaxLength);
            letter.RuleFor(x => x.Subject).NotEmpty().WithMessage("Вкажіть тему варіанта листа.").MaximumLength(L.ShortTextMaxLength);
            letter.RuleFor(x => x.Body).NotEmpty().WithMessage("Вкажіть текст варіанта листа.").MaximumLength(L.TextMaxLength);
        });
        RuleFor(x => x.StaffContacts).NotNull()
            .Must(x => x is { Count: <= L.MaxStaffContacts })
            .WithMessage("Забагато контактів.")
            .Must(x => x is null || x.Select(c => c.UserId).Distinct().Count() == x.Count)
            .WithMessage("Контакт співробітника вказано двічі.");
        RuleForEach(x => x.StaffContacts).NotNull().ChildRules(contact =>
        {
            contact.RuleFor(x => x.UserId).GreaterThan(0);
            contact.RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть ім'я менеджера для КП.").MaximumLength(L.ShortTextMaxLength);
            contact.RuleFor(x => x.Phone).MaximumLength(50);
            contact.RuleFor(x => x.Email).EmailAddress().WithMessage("Перевірте email менеджера.")
                .MaximumLength(L.ShortTextMaxLength).When(x => !string.IsNullOrWhiteSpace(x.Email));
        });
    }

    internal static bool IsWebUrl(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http";
}

public sealed class QuoteCompanyValidator : AbstractValidator<QuoteCompanyDto>
{
    public QuoteCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть повну назву підприємства.").MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.Edrpou).Matches(@"^\s*\d{8}\s*$").WithMessage("ЄДРПОУ — 8 цифр.")
            .When(x => !string.IsNullOrWhiteSpace(x.Edrpou));
        RuleFor(x => x.TaxId).Matches(@"^\s*\d{10,12}\s*$").WithMessage("ІПН — від 10 до 12 цифр.")
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));
        RuleFor(x => x.Iban).Matches(@"^\s*UA\d{2}[\d ]{20,40}\s*$").WithMessage("IBAN починається з UA і містить цифри.")
            .When(x => !string.IsNullOrWhiteSpace(x.Iban));
        RuleFor(x => x.Address).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Email).EmailAddress().WithMessage("Перевірте email Brevi.").MaximumLength(L.ShortTextMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Site).Must(SaveQuoteSettingsInputValidator.IsWebUrl).WithMessage("Адреса сайту — посилання https://…")
            .MaximumLength(L.UrlMaxLength);
        RuleFor(x => x.LogoUrl).Must(SaveQuoteSettingsInputValidator.IsWebUrl).WithMessage("Логотип — посилання https://…")
            .MaximumLength(L.UrlMaxLength);
    }
}

public sealed class QuoteIntroductionValidator : AbstractValidator<QuoteIntroductionTextsDto>
{
    public QuoteIntroductionValidator()
    {
        RuleFor(x => x.LogoText).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.SummerTitle).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.WinterTitle).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.Subtitle).MaximumLength(L.TextMaxLength);
        RuleFor(x => x.ModelsTitle).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.AssortmentLinkText).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.ProductButtonText).MaximumLength(L.ShortTextMaxLength);
        RuleFor(x => x.NextStepText).MaximumLength(L.TextMaxLength);
        RuleFor(x => x.SummerBannerUrl).Must(SaveQuoteSettingsInputValidator.IsWebUrl)
            .WithMessage("Фото банера — посилання https://…").MaximumLength(L.UrlMaxLength);
        RuleFor(x => x.WinterBannerUrl).Must(SaveQuoteSettingsInputValidator.IsWebUrl)
            .WithMessage("Фото банера — посилання https://…").MaximumLength(L.UrlMaxLength);
        RuleFor(x => x.AssortmentUrl).Must(SaveQuoteSettingsInputValidator.IsWebUrl)
            .WithMessage("Посилання на асортимент — https://…").MaximumLength(L.UrlMaxLength);
        RuleFor(x => x.SummerMainCode).MaximumLength(QuoteLimits.CodeMaxLength);
        RuleFor(x => x.WinterMainCode).MaximumLength(QuoteLimits.CodeMaxLength);
        RuleFor(x => x.AccentColor).Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Акцентний колір — у форматі #RRGGBB.")
            .When(x => !string.IsNullOrWhiteSpace(x.AccentColor));
        RuleFor(x => x.Advantages).NotNull()
            .Must(x => x is { Count: L.AdvantagesCount })
            .WithMessage($"Переваг має бути рівно {L.AdvantagesCount}.");
        RuleForEach(x => x.Advantages).NotNull().ChildRules(advantage =>
        {
            advantage.RuleFor(x => x.Title).NotEmpty().WithMessage("Вкажіть заголовок переваги.").MaximumLength(L.ShortTextMaxLength);
            advantage.RuleFor(x => x.Text).MaximumLength(L.ShortTextMaxLength);
        });
    }
}
