using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Sending.Send;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;

namespace Crm.Application.Quotes.Introductions.Validation;

/// <summary>
/// The shape of the pages (R-16): one page per season of the layout in order, four models each, no model twice.
/// Which model is the main one and its season need the Catalog and are checked by the builder.
/// </summary>
internal static class QuoteIntroPagesRules
{
    public static bool IsLayout(string? value) =>
        Enum.TryParse<QuoteIntroLayout>(value, false, out var layout) && Enum.IsDefined(layout);

    public static bool MatchLayout(string layout, IReadOnlyList<QuoteIntroPageInput>? pages)
    {
        if (pages is null || !IsLayout(layout)) return true;
        var expected = new QuoteIntroduction(Enum.Parse<QuoteIntroLayout>(layout), null, null).Pages
            .Select(x => x.ToString());
        return pages.Select(x => x.Season).SequenceEqual(expected);
    }

    public static bool Distinct(IReadOnlyList<QuoteIntroPageInput>? pages) =>
        pages is null || pages.Where(x => x?.Models is not null).SelectMany(x => x.Models)
            .Where(x => x is not null).GroupBy(x => x.ProductId).All(x => x.Count() == 1);

    public static void Apply<T>(AbstractValidator<T> validator, Func<T, string> layout,
        Func<T, IReadOnlyList<QuoteIntroPageInput>> pages)
    {
        validator.RuleFor(x => layout(x)).Must(IsLayout).WithMessage("Оберіть основу добірки.")
            .OverridePropertyName("Layout");
        validator.RuleFor(x => pages(x)).NotNull()
            .Must((input, value) => MatchLayout(layout(input), value))
            .WithMessage("Сторінки не відповідають основі добірки.")
            .Must(Distinct).WithMessage("Одна модель може бути в добірці лише раз.")
            .OverridePropertyName("Pages");
        validator.RuleForEach(x => pages(x)).NotNull().SetValidator(new QuoteIntroPageInputValidator())
            .OverridePropertyName("Pages");
    }
}

public sealed class QuoteIntroPageInputValidator : AbstractValidator<QuoteIntroPageInput>
{
    public QuoteIntroPageInputValidator()
    {
        RuleFor(x => x.Models).NotNull()
            .Must(x => x is { Count: QuoteLimits.IntroModelsPerPage })
            .WithMessage($"Оберіть головну модель і ще {QuoteLimits.IntroModelsPerPage - 1} моделі на кожну сторінку.");
        RuleForEach(x => x.Models).NotNull().ChildRules(model =>
        {
            model.RuleFor(x => x.ProductId).GreaterThan(0);
            model.RuleFor(x => x.ArticleId).GreaterThan(0).WithMessage("Оберіть тканину для кожної моделі.");
            model.RuleFor(x => x.ItemId).GreaterThan(0).When(x => x.ItemId is not null);
        });
    }
}

public sealed class SaveQuoteIntroductionInputValidator : AbstractValidator<SaveQuoteIntroductionInput>
{
    public SaveQuoteIntroductionInputValidator()
    {
        RuleFor(x => x.InternalName).NotEmpty().WithMessage("Вкажіть клієнта або назву запиту.")
            .MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId is not null);
        RuleFor(x => x.RecipientEmail).MaximumLength(QuoteLimits.EmailMaxLength)
            .Must(x => SendQuoteCommandValidator.IsEmail(x!.Trim())).WithMessage("Перевірте email отримувача.")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientEmail));
        RuleFor(x => x.RecipientName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.CompanyName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.ManagerNote).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.LetterName).MaximumLength(QuoteLimits.LetterNameMaxLength);
        RuleFor(x => x.LetterSubject).MaximumLength(QuoteLimits.LetterSubjectMaxLength)
            .WithMessage($"Тема листа — до {QuoteLimits.LetterSubjectMaxLength} символів.");
        RuleFor(x => x.LetterBody).MaximumLength(QuoteLimits.LetterBodyMaxLength)
            .WithMessage($"Текст листа — до {QuoteLimits.LetterBodyMaxLength} символів.");
        QuoteIntroPagesRules.Apply(this, x => x.Layout, x => x.Pages);
    }
}

public sealed class CreateQuoteIntroTemplateInputValidator : AbstractValidator<CreateQuoteIntroTemplateInput>
{
    public CreateQuoteIntroTemplateInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву шаблону.")
            .MaximumLength(QuoteIntroTemplateLimits.NameMaxLength);
        RuleFor(x => x.LetterName).MaximumLength(QuoteLimits.LetterNameMaxLength);
        QuoteIntroPagesRules.Apply(this, x => x.Layout, x => x.Pages);
    }
}
