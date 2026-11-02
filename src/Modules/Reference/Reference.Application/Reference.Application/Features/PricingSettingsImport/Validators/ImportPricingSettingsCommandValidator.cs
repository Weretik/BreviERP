namespace Reference.Application.Features.PricingSettingsImport.Validators;

/// <summary>Checks the file as a whole; problems of a single row are reported in the preview instead.</summary>
public sealed class ImportPricingSettingsCommandValidator : AbstractValidator<ImportPricingSettingsCommand>
{
    public const int MaxRows = 1000;

    public ImportPricingSettingsCommandValidator()
    {
        RuleFor(x => x.Parameters)
            .NotEmpty().WithMessage("У файлі немає параметрів.")
            .Must(rows => rows.Count <= MaxRows).WithMessage($"Не більше {MaxRows} параметрів за один імпорт.")
            .Must(rows => rows.All(row => !string.IsNullOrWhiteSpace(row.Code)))
            .WithMessage("У кожного параметра має бути код.")
            .Must(rows => rows.Select(row => row.Code.Trim().ToUpperInvariant()).Distinct().Count() == rows.Count)
            .WithMessage("Коди параметрів у файлі повторюються.");

        RuleFor(x => x.PpeMarkupRules)
            .NotNull().WithMessage("Список правил ЗІЗ не може бути порожнім значенням.")
            .Must(rows => rows.Count <= MaxRows).WithMessage($"Не більше {MaxRows} правил за один імпорт.")
            .Must(rows => rows.All(row => !string.IsNullOrWhiteSpace(row.Name)))
            .WithMessage("У кожного правила ЗІЗ має бути назва.")
            .Must(rows => rows.Select(row => row.Name.Trim()).Distinct().Count() == rows.Count)
            .WithMessage("Назви правил ЗІЗ у файлі повторюються.");
    }
}
