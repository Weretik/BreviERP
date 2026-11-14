using Reference.Application.Features.SheetImport.Shared;

namespace Reference.Application.Features.ProductSection.Commands.ImportProductSections;

/// <summary>Checks the file as a whole; problems of a single row are reported in the preview instead.</summary>
public sealed class ImportProductSectionsValidator : AbstractValidator<ImportProductSectionsCommand>
{
    public const int MaxRows = 1000;

    public ImportProductSectionsValidator()
    {
        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage("У файлі немає розділів.")
            .Must(rows => rows.Count <= MaxRows).WithMessage($"Не більше {MaxRows} розділів за один імпорт.")
            .Must(rows => rows.All(row => !string.IsNullOrWhiteSpace(row.Name)))
            .WithMessage("У кожного розділу має бути назва.")
            .Must(rows => rows.Select(row => ReferenceNameKey.From(row.Name)).Distinct().Count() == rows.Count)
            .WithMessage("Назви розділів у файлі повторюються.");
    }
}
