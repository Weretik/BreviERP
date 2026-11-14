using Crm.Domain.Quotes;

namespace Crm.Application.Features.Quotes.Commands.ImportQuotes;

/// <summary>Shape of the transfer file; links to the Catalog and Reference are checked by the mapper as warnings.</summary>
public sealed class ImportQuotesValidator : AbstractValidator<ImportQuotesCommand>
{
    public const int MaxQuotes = 1000;

    public ImportQuotesValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Quotes).NotNull()
            .Must(x => x is { Count: > 0 and <= MaxQuotes })
            .WithMessage($"Передайте від 1 до {MaxQuotes} КП.");
        RuleForEach(x => x.Quotes).NotNull().SetValidator(new ImportQuoteInputValidator());
        RuleFor(x => x.Quotes)
            .Must(x => x.Where(q => q?.Number is not null).GroupBy(q => q.Number.Trim()).All(g => g.Count() == 1))
            .When(x => x.Quotes is not null).WithMessage("Номери КП у файлі повторюються.");
    }
}

public sealed class ImportQuoteInputValidator : AbstractValidator<ImportQuoteInput>
{
    public ImportQuoteInputValidator()
    {
        RuleFor(x => x.Number).Must(QuoteImportNumber.IsValid).WithMessage("Номер КП має вигляд «номер-місяць-рік».");
        RuleFor(x => x.Kind).Must(IsEnum<QuoteKind>).WithMessage("Невідомий вид КП.");
        RuleFor(x => x.Status).Must(IsEnum<QuoteStatus>).WithMessage("Невідомий статус КП.");
        RuleFor(x => x.InternalName).NotEmpty().MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.RecipientName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.CompanyName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.ClientComment).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.ManagerNote).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.KeepinClientId).MaximumLength(200);
        RuleFor(x => x.Author).NotNull().SetValidator(new ImportPersonInputValidator());
        RuleFor(x => x.Responsible).NotNull().SetValidator(new ImportPersonInputValidator());
        RuleFor(x => x.Introduction!.Layout).Must(IsEnum<QuoteIntroLayout>).When(x => x.Introduction is not null)
            .WithMessage("Невідомий макет КП для знайомства.");
        RuleFor(x => x.Introduction!.RecipientEmail).MaximumLength(QuoteLimits.EmailMaxLength)
            .When(x => x.Introduction is not null);
        RuleFor(x => x.Items).NotNull()
            .Must(x => x is { Count: <= QuoteLimits.MaxItems })
            .WithMessage($"У КП може бути не більше {QuoteLimits.MaxItems} позицій.");
        RuleForEach(x => x.Items).NotNull().SetValidator(new ImportItemInputValidator());
        RuleForEach(x => x.Events).NotNull().ChildRules(e =>
        {
            e.RuleFor(x => x.Kind).Must(x => x is nameof(QuoteEventKind.Sent) or nameof(QuoteEventKind.ResponsibleChanged))
                .WithMessage("В історії переносяться лише відправлення та зміни відповідального.");
            e.RuleFor(x => x.Actor).NotNull().SetValidator(new ImportPersonInputValidator());
        });
    }

    internal static bool IsEnum<T>(string? value) where T : struct, Enum =>
        Enum.TryParse<T>(value, false, out var parsed) && Enum.IsDefined(parsed);
}

public sealed class ImportPersonInputValidator : AbstractValidator<ImportPersonInput>
{
    public ImportPersonInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.Email).MaximumLength(QuoteLimits.EmailMaxLength);
    }
}

public sealed class ImportItemInputValidator : AbstractValidator<ImportItemInput>
{
    public ImportItemInputValidator()
    {
        RuleFor(x => x.Kind).Must(x => ImportQuoteInputValidator.IsEnum<QuoteItemKind>(x) && x != nameof(QuoteItemKind.Embroidery))
            .WithMessage("Невідомий тип позиції.");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(QuoteLimits.CodeMaxLength)
            .When(x => x.Kind is nameof(QuoteItemKind.Sewing) or nameof(QuoteItemKind.Ppe));
        RuleFor(x => x.Name).NotEmpty().MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.SiteUrl).MaximumLength(QuoteLimits.UrlMaxLength);
        RuleFor(x => x.PhotoUrl).MaximumLength(QuoteLimits.UrlMaxLength);
        RuleFor(x => x.SupplierUrl).MaximumLength(QuoteLimits.UrlMaxLength);
        RuleFor(x => x.Season).MaximumLength(QuoteLimits.CodeMaxLength);
        RuleFor(x => x.Options).NotNull()
            .Must(x => x is { Count: > 0 and <= QuoteLimits.MaxOptionsPerItem })
            .WithMessage($"Позиція має мати від 1 до {QuoteLimits.MaxOptionsPerItem} рядків цін.");
        RuleFor(x => x.Options).Must(x => x.Count(o => o?.IsRecommended == true) <= 1)
            .When(x => x.Options is not null).WithMessage("Рекомендованим може бути лише один варіант.");
        RuleForEach(x => x.Options).NotNull().SetValidator(new ImportOptionInputValidator());
        RuleFor(x => x.Lines).Must(x => x is null || x.Count <= QuoteLimits.MaxCompositionLines)
            .WithMessage($"Не більше {QuoteLimits.MaxCompositionLines} рядків складу.");
        RuleForEach(x => x.Lines).NotNull().ChildRules(line =>
        {
            line.RuleFor(x => x.Kind).Must(x => x is nameof(QuoteItemLineKind.Fabric) or nameof(QuoteItemLineKind.Accessory)
                or nameof(QuoteItemLineKind.Operation)).WithMessage("Невідомий тип рядка складу.");
            line.RuleFor(x => x.Code).NotEmpty().MaximumLength(QuoteLimits.CodeMaxLength);
            line.RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(QuoteLimits.MaxCompositionQuantity);
        });
    }
}

public sealed class ImportOptionInputValidator : AbstractValidator<ImportOptionInput>
{
    public ImportOptionInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.Details).MaximumLength(QuoteLimits.TextMaxLength);
        RuleFor(x => x.FabricCode).MaximumLength(QuoteLimits.CodeMaxLength);
        RuleFor(x => x.FabricMeters).GreaterThan(0).LessThanOrEqualTo(QuoteLimits.MaxCompositionQuantity)
            .When(x => x.FabricMeters is not null);
        RuleFor(x => x.ManualReason).MaximumLength(QuoteLimits.ReasonMaxLength);
        RuleFor(x => x.ManualByEmail).MaximumLength(QuoteLimits.EmailMaxLength);
        RuleFor(x => x.ManualByName).MaximumLength(QuoteLimits.NameMaxLength);
        RuleFor(x => x.Prices).NotNull();
        When(x => x.Prices is not null, () =>
        {
            RuleFor(x => x.Prices.Retail).NotNull().Must(IsPrice).WithMessage(PriceMessage);
            RuleFor(x => x.Prices.Wholesale).NotNull().Must(IsPrice).WithMessage(PriceMessage);
            RuleFor(x => x.Prices.Large).NotNull().Must(IsPrice).WithMessage(PriceMessage);
        });
    }

    private const string PriceMessage = "Ціна має бути від 0 до 10 000 000.";

    private static bool IsPrice(ImportPriceInput? price) => price is null ||
        (price.Source is null or >= 0 and <= QuoteLimits.MaxPrice && price.Manual is null or >= 0 and <= QuoteLimits.MaxPrice);
}
