using Crm.Application.Quotes.Contracts;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Validation;

/// <summary>Shape of a «Виріб з нуля» composition; prices and reference rows are checked by the calculator.</summary>
public static class QuoteCompositionRules
{
    public const string MetersMessage = "Вкажіть метраж тканини більше нуля.";
    public static readonly string TooManyLinesMessage =
        $"Не більше {QuoteLimits.MaxCompositionLines} рядків складу.";

    public static bool IsQuantity(decimal value) => value > 0 && value <= QuoteLimits.MaxCompositionQuantity;

    public static bool IsQuantity(decimal? value) => value is { } quantity && IsQuantity(quantity);
}

public sealed class QuoteItemLineInputValidator : AbstractValidator<QuoteItemLineInput>
{
    public QuoteItemLineInputValidator()
    {
        RuleFor(x => x.Kind).Must(x => Enum.TryParse<QuoteItemLineKind>(x, false, out var kind) && Enum.IsDefined(kind))
            .WithMessage("Невідомий тип рядка складу.");
        RuleFor(x => x.ReferenceId).GreaterThan(0).When(x => x.Kind != nameof(QuoteItemLineKind.Service));
        RuleFor(x => x.CostCode).NotEmpty().WithMessage("Вкажіть код послуги.")
            .MaximumLength(QuoteLimits.CostCodeMaxLength)
            .When(x => x.Kind == nameof(QuoteItemLineKind.Service));
        RuleFor(x => x.CostCode).Null().When(x => x.Kind != nameof(QuoteItemLineKind.Service))
            .WithMessage("Код задається лише для послуги.");
        RuleFor(x => x.Quantity).Must(QuoteCompositionRules.IsQuantity)
            .WithMessage("Кількість має бути більше нуля.");
        RuleFor(x => x.ManualMinutes).Must(QuoteCompositionRules.IsQuantity)
            .When(x => x.ManualMinutes is not null).WithMessage("Хвилини операції мають бути більше нуля.");
        RuleFor(x => x.ManualMinutes).Null().When(x => x.Kind != nameof(QuoteItemLineKind.Operation))
            .WithMessage("Хвилини задаються лише для операції.");
    }
}

/// <summary>The fabrics of one variant: 1–10 different fabrics, each with its metres.</summary>
public sealed class QuoteOptionFabricsValidator : AbstractValidator<IReadOnlyList<QuoteOptionFabricInput>?>
{
    public const int MaxFabrics = 10;

    public QuoteOptionFabricsValidator()
    {
        RuleFor(x => x).NotNull().Must(x => x is { Count: > 0 and <= MaxFabrics })
            .WithMessage($"Варіант має від 1 до {MaxFabrics} тканин.");
        RuleForEach(x => x).NotNull().ChildRules(fabric =>
        {
            fabric.RuleFor(x => x.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
            fabric.RuleFor(x => x.Meters).Must(QuoteCompositionRules.IsQuantity)
                .WithMessage(QuoteCompositionRules.MetersMessage);
        });
        RuleFor(x => x).Must(x => x!.Where(f => f is not null).Select(f => f.FabricId).Distinct().Count() == x!.Count)
            .When(x => x is not null).WithMessage("Тканина додана у варіант двічі.");
    }

    /// <summary>The variant as a comparable key: fabrics and metres in order.</summary>
    public static string Key(IReadOnlyList<QuoteOptionFabricInput>? fabrics) =>
        string.Join("|", (fabrics ?? []).Select(x => $"{x.FabricId}:{x.Meters}"));
}
