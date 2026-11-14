namespace Reference.Application.Features.GarmentAccessory.Commands.ImportGarmentAccessories;

/// <summary>Checks the file as a whole; problems of a single row are reported in the preview instead.</summary>
public sealed class ImportGarmentAccessoriesValidator : AbstractValidator<ImportGarmentAccessoriesCommand>
{
    public const int MaxRows = 2000;

    public ImportGarmentAccessoriesValidator()
    {
        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage("У файлі немає рядків з фурнітурою.")
            .Must(rows => rows.Count <= MaxRows).WithMessage($"Не більше {MaxRows} рядків за один імпорт.")
            .Must(rows => rows.All(row => !string.IsNullOrWhiteSpace(row.Code)))
            .WithMessage("У кожного рядка має бути ID фурнітури (FUR-…).")
            .Must(rows => rows.Select(row => row.Code.Trim().ToUpperInvariant()).Distinct().Count() == rows.Count)
            .WithMessage("ID фурнітури в файлі повторюються.");
    }
}
