namespace Reference.Application.Features.GarmentPartOperation.Import.Validators;

/// <summary>Checks the file as a whole; problems of a single row are reported in the preview instead.</summary>
public sealed class ImportGarmentPartOperationsCommandValidator : AbstractValidator<ImportGarmentPartOperationsCommand>
{
    public const int MaxRows = 2000;

    public ImportGarmentPartOperationsCommandValidator()
    {
        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage("У файлі немає рядків з операціями.")
            .Must(rows => rows.Count <= MaxRows).WithMessage($"Не більше {MaxRows} рядків за один імпорт.")
            .Must(rows => rows.All(row => !string.IsNullOrWhiteSpace(row.Code)))
            .WithMessage("У кожного рядка має бути ID операції (OP-…).")
            .Must(rows => rows.Select(row => row.Code.Trim().ToUpperInvariant()).Distinct().Count() == rows.Count)
            .WithMessage("ID операцій у файлі повторюються.");
    }
}
