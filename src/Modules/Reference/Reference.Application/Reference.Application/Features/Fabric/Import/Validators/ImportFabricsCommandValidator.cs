namespace Reference.Application.Features.Fabric.Import.Validators;

/// <summary>Checks the file as a whole; problems of a single row are reported in the preview instead.</summary>
public sealed class ImportFabricsCommandValidator : AbstractValidator<ImportFabricsCommand>
{
    public const int MaxRows = 2000;

    public ImportFabricsCommandValidator()
    {
        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage("У файлі немає рядків з тканинами.")
            .Must(rows => rows.Count <= MaxRows).WithMessage($"Не більше {MaxRows} рядків за один імпорт.")
            .Must(rows => rows.All(row => !string.IsNullOrWhiteSpace(row.Code)))
            .WithMessage("У кожного рядка має бути ID тканини (FAB-…).")
            .Must(rows => rows.Select(row => row.Code.Trim().ToUpperInvariant()).Distinct().Count() == rows.Count)
            .WithMessage("ID тканини в файлі повторюються.");
    }
}
