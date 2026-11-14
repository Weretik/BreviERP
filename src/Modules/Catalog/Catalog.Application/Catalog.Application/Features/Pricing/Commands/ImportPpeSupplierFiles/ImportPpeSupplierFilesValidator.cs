namespace Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;

public sealed class ImportPpeSupplierFilesValidator : AbstractValidator<ImportPpeSupplierFilesCommand>
{
    public ImportPpeSupplierFilesValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Files).NotEmpty().WithMessage("Додайте файл прайсу.")
            .Must(files => files.Count <= 3).WithMessage("Не більше трьох файлів.");
        RuleFor(x => x.Actor).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ActorName).NotEmpty().MaximumLength(200);
    }
}
