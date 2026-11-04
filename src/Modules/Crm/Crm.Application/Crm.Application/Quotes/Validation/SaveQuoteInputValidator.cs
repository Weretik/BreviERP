using Crm.Application.Quotes.Contracts;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Validation;

/// <summary>Shape of the editor content shared by create and save; price rules that need the Catalog live in the builder.</summary>
public sealed class SaveQuoteInputValidator : AbstractValidator<SaveQuoteInput>
{
    public SaveQuoteInputValidator()
    {
        RuleFor(x => x.InternalName).NotEmpty().WithMessage("Вкажіть клієнта або назву запиту.")
            .MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId is not null);
        RuleFor(x => x.RecipientName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.CompanyName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.ClientComment).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.ManagerNote).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.Items).NotNull()
            .Must(x => x is { Count: <= QuoteLimits.MaxItems })
            .WithMessage($"У КП може бути не більше {QuoteLimits.MaxItems} позицій.");
        RuleForEach(x => x.Items).NotNull().SetValidator(new QuoteItemInputValidator());
    }
}

public sealed class QuoteItemInputValidator : AbstractValidator<QuoteItemInput>
{
    public QuoteItemInputValidator()
    {
        RuleFor(x => x.Kind).Must(x => Enum.TryParse<QuoteItemKind>(x, false, out var kind) && Enum.IsDefined(kind))
            .WithMessage("Невідомий тип позиції.");
        RuleFor(x => x.Description).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.PhotoUrl).MaximumLength(QuoteLimits.UrlMaxLength);
        RuleFor(x => x.SupplierUrl).MaximumLength(QuoteLimits.UrlMaxLength);
        RuleFor(x => x.Options).NotNull()
            .Must(x => x is { Count: > 0 and <= QuoteLimits.MaxOptionsPerItem })
            .WithMessage($"Позиція має мати від 1 до {QuoteLimits.MaxOptionsPerItem} рядків цін.");
        RuleForEach(x => x.Options).NotNull().SetValidator(new QuoteOptionInputValidator());
        RuleFor(x => x.Options).Must(x => x.Count(o => o.IsRecommended) <= 1)
            .When(x => x.Options is not null).WithMessage("Рекомендованим може бути лише один варіант.");

        When(x => x.Kind == nameof(QuoteItemKind.OneOffPpe), () =>
        {
            RuleFor(x => x.ProductId).Null();
            RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву разової позиції.")
                .MaximumLength(QuoteLimits.NameMaxLength);
            RuleFor(x => x.Options).Must(x => x.Count == 1).When(x => x.Options is not null)
                .WithMessage("Разова позиція має один рядок цін.");
        });
        When(x => x.Kind is nameof(QuoteItemKind.Sewing) or nameof(QuoteItemKind.Ppe), () =>
        {
            RuleFor(x => x.ProductId).NotNull().GreaterThan(0);
        });
        When(x => x.Kind == nameof(QuoteItemKind.Sewing), () =>
        {
            RuleForEach(x => x.Options).Must(NoManual)
                .WithMessage("Ціну швейної моделі не можна змінити вручну.")
                .Must(IsModelRow)
                .WithMessage("Оберіть варіант тканини з каталогу або іншу тканину з метражем.");
            RuleForEach(x => x.Options).ChildRules(option =>
                option.RuleFor(x => x.Fabrics).SetValidator(new QuoteOptionFabricsValidator())
                    .When(x => x.Fabrics is { Count: > 0 }));
            RuleFor(x => x.Options)
                .Must(x => x.Where(o => o?.ArticleId is not null).GroupBy(o => o.ArticleId).All(g => g.Count() == 1))
                .When(x => x.Options is not null).WithMessage("Варіант тканини доданий двічі.")
                .Must(x => x.Where(o => o?.Fabrics is { Count: > 0 }).GroupBy(o => o.Fabrics![0].FabricId)
                    .All(g => g.Count() == 1))
                .When(x => x.Options is not null).WithMessage("Тканина додана двічі.");
        });
        When(x => x.Kind == nameof(QuoteItemKind.CustomSewing), () =>
        {
            RuleFor(x => x.ProductId).GreaterThan(0).When(x => x.ProductId is not null);
            RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву виробу з нуля.")
                .MaximumLength(QuoteLimits.NameMaxLength);
            RuleFor(x => x.Lines).NotNull()
                .Must(x => x is { Count: <= QuoteLimits.MaxCompositionLines })
                .WithMessage(QuoteCompositionRules.TooManyLinesMessage)
                .Must(x => x!.Any(line => line?.Kind == nameof(QuoteItemLineKind.Operation)))
                .WithMessage("Додайте до виробу з нуля хоча б одну операцію.");
            RuleForEach(x => x.Lines).NotNull().SetValidator(new QuoteItemLineInputValidator());
            RuleForEach(x => x.Options).Must(x => x.ArticleId is null)
                .Must(NoManual).WithMessage("Ціну виробу з нуля рахує система; змініть склад, щоб змінити ціну.");
            RuleForEach(x => x.Options).ChildRules(option =>
                option.RuleFor(x => x.Fabrics).SetValidator(new QuoteOptionFabricsValidator()));
            RuleFor(x => x.Options)
                .Must(x => x.Select(o => QuoteOptionFabricsValidator.Key(o?.Fabrics)).Distinct().Count() == x.Count)
                .When(x => x.Options is not null).WithMessage("Такий варіант тканин уже є.");
        });
        RuleFor(x => x.Lines).Must(x => x is null || x.Count == 0)
            .When(x => x.Kind != nameof(QuoteItemKind.CustomSewing))
            .WithMessage("Склад задається лише для виробу з нуля.");
        When(x => x.Kind == nameof(QuoteItemKind.Ppe), () =>
        {
            RuleFor(x => x.Options).Must(x => x.Count == 1).When(x => x.Options is not null)
                .WithMessage("Товар ЗІЗ має один рядок цін.");
            RuleForEach(x => x.Options).Must(x => x.ArticleId is null);
        });
    }

    /// <summary>
    /// A Catalog variant, a model row in another fabric (R-27) or a saved row whose variant is gone from the Catalog
    /// (a transferred КП, R-25), which keeps its prices.
    /// </summary>
    private static bool IsModelRow(QuoteOptionInput input) => input.ArticleId switch
    {
        > 0 => input.Fabrics is null or { Count: 0 },
        null => input.Fabrics is { Count: 1 } || (input.Id is > 0 && input.Fabrics is null or { Count: 0 }),
        _ => false,
    };

    private static bool NoManual(QuoteOptionInput input) =>
        input.Retail?.Manual is null && input.Wholesale?.Manual is null && input.Large?.Manual is null;
}

public sealed class QuoteOptionInputValidator : AbstractValidator<QuoteOptionInput>
{
    public QuoteOptionInputValidator()
    {
        RuleFor(x => x.Retail).NotNull().SetValidator(new QuotePriceInputValidator());
        RuleFor(x => x.Wholesale).NotNull().SetValidator(new QuotePriceInputValidator());
        RuleFor(x => x.Large).NotNull().SetValidator(new QuotePriceInputValidator());
        RuleFor(x => x.ManualReason).MaximumLength(QuoteLimits.ReasonMaxLength);
    }
}

public sealed class QuotePriceInputValidator : AbstractValidator<QuotePriceInput>
{
    public QuotePriceInputValidator()
    {
        RuleFor(x => x.Manual).GreaterThanOrEqualTo(0).LessThanOrEqualTo(QuoteLimits.MaxPrice)
            .PrecisionScale(12, 2, true).When(x => x.Manual is not null)
            .WithMessage("Ціна має бути від 0 до 10 000 000 грн з двома знаками після коми.");
    }
}
