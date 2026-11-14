using Crm.Domain.Embroidery.Settings;
using R = Crm.Domain.Embroidery.Settings.EmbroiderySettingsRules;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Validation;

namespace Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;

public sealed class SaveEmbroiderySettingsValidator : AbstractValidator<SaveEmbroiderySettingsCommand>
{
    public SaveEmbroiderySettingsValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveEmbroiderySettingsInputValidator());
    }
}

/// <summary>The same rules as <see cref="EmbroiderySettingsRules"/>, with Ukrainian messages for the page.</summary>
/// <param name="markupCount">Two on the page; the old calculator's archive has three.</param>
public sealed class SaveEmbroiderySettingsInputValidator : AbstractValidator<SaveEmbroiderySettingsInput>
{
    public SaveEmbroiderySettingsInputValidator(int markupCount = R.MarkupCount)
    {
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Rates).NotNull().ChildRules(rates =>
        {
            Amount(rates.RuleFor(x => x.ThreadRate), "Нитки");
            Amount(rates.RuleFor(x => x.HoopingRate), "Зап’ялювання");
            Amount(rates.RuleFor(x => x.ProgramHourRate), "Розробка програми");
            Amount(rates.RuleFor(x => x.TaxPercent), "Податкове навантаження");
        });
        RuleFor(x => x.Markups).NotNull().Must(x => x is not null && x.Count == markupCount)
            .WithMessage(markupCount == R.MarkupCount ? "Має бути рівно дві категорії ціни." : "Має бути рівно три націнки.");
        RuleForEach(x => x.Markups).NotNull().ChildRules(markup =>
        {
            markup.RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву категорії.").MaximumLength(R.NameMaxLength);
            Amount(markup.RuleFor(x => x.Percent), "Націнка");
        });
        RuleFor(x => x.Stabilizer).NotNull().SetValidator(new MaterialValidator("Флізелін"));
        RuleFor(x => x.Fabric).NotNull().SetValidator(new MaterialValidator("Тканина"));
        Amount(RuleFor(x => x.FabricIncreasePercent), "Запас тканини");

        RuleFor(x => x.Hoops).NotNull().Must(x => x is { Count: >= R.MinHoops and <= R.MaxHoops })
            .WithMessage($"Потрібно від {R.MinHoops} до {R.MaxHoops} п’ялець.")
            .Must(x => x is null || Unique(x.Select(h => h?.Id))).WithMessage("Код п’ялець повторюється.");
        RuleForEach(x => x.Hoops).NotNull().ChildRules(hoop =>
        {
            hoop.RuleFor(x => x.Id).NotEmpty().MaximumLength(R.IdMaxLength);
            hoop.RuleFor(x => x.Label).NotEmpty().WithMessage("Вкажіть назву п’ялець.").MaximumLength(R.NameMaxLength);
            hoop.RuleFor(x => x.BlankWidthCm).GreaterThan(0).WithMessage("Ширина заготовки має бути більше нуля.")
                .LessThanOrEqualTo(R.MaxAmount);
            hoop.RuleFor(x => x.BlankLengthCm).GreaterThan(0).WithMessage("Довжина заготовки має бути більше нуля.")
                .LessThanOrEqualTo(R.MaxAmount);
            hoop.RuleFor(x => x.GlueRate!.Value).InclusiveBetween(0, R.MaxAmount)
                .WithMessage("Клей для п’ялець не може бути від’ємним.").When(x => x.GlueRate is not null);
        });

        RuleFor(x => x.Expenses).NotNull().Must(x => x is { Count: <= R.MaxExpenses })
            .WithMessage($"Не більше {R.MaxExpenses} статей.")
            .Must(x => x is null || Unique(x.Select(e => e?.Id))).WithMessage("Код статті повторюється.");
        RuleForEach(x => x.Expenses).NotNull().ChildRules(expense =>
        {
            expense.RuleFor(x => x.Id).NotEmpty().MaximumLength(R.IdMaxLength);
            expense.RuleFor(x => x.Name).NotEmpty().WithMessage("Вкажіть назву статті.").MaximumLength(R.NameMaxLength);
            expense.RuleFor(x => x.Basis).Must(EmbroideryCodes.Is<EmbroideryExpenseBasis>)
                .WithMessage("Оберіть, за що рахується стаття.");
            expense.RuleFor(x => x.Mode).Must(EmbroideryCodes.Is<EmbroideryExpenseMode>)
                .WithMessage("Оберіть, де діє стаття.");
            expense.RuleFor(x => x.Rate!.Value).InclusiveBetween(0, R.MaxAmount)
                .WithMessage("Ставка не може бути від’ємною.").When(x => x.Rate is not null);
        });
    }

    private static void Amount<T>(IRuleBuilderInitial<T, decimal> rule, string name) =>
        rule.InclusiveBetween(0, R.MaxAmount).WithMessage($"{name}: число від 0 до {R.MaxAmount:0}.");

    private static bool Unique(IEnumerable<string?> ids)
    {
        var list = ids.Select(x => x?.Trim()).ToList();
        return list.Distinct(StringComparer.Ordinal).Count() == list.Count;
    }

    private sealed class MaterialValidator : AbstractValidator<EmbroideryMaterialDto>
    {
        public MaterialValidator(string name)
        {
            RuleFor(x => x.PricePerMetre).InclusiveBetween(0, R.MaxAmount)
                .WithMessage($"{name}: ціна від 0 до {R.MaxAmount:0}.");
            RuleFor(x => x.WidthCm).GreaterThan(0).WithMessage($"{name}: ширина рулону має бути більше нуля.")
                .LessThanOrEqualTo(R.MaxAmount);
        }
    }
}
