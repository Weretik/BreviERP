namespace Catalog.Application.Features.Product.SheetImport.Validators;

/// <summary>Checks the body as a whole; problems of single rows are reported in the import report instead.</summary>
public sealed class ImportSheetProductsCommandValidator : AbstractValidator<ImportSheetProductsCommand>
{
    public const int MaxRows = 2000;

    public ImportSheetProductsCommandValidator()
    {
        RuleFor(x => x.Sewing).NotNull().WithMessage("Немає списку швейних моделей (sewing).");
        RuleFor(x => x.Ppe).NotNull().WithMessage("Немає списку ЗІЗ (ppe).");
        RuleFor(x => x.Actor).NotEmpty();
        When(x => x.Sewing is not null && x.Ppe is not null, () =>
        {
            RuleFor(x => x.Sewing.Count + x.Ppe.Count).GreaterThan(0).WithName("rows")
                .WithMessage("У файлі немає товарів.");
            RuleFor(x => x.Sewing.Count).LessThanOrEqualTo(MaxRows).WithName("sewing")
                .WithMessage($"Не більше {MaxRows} моделей за один імпорт.");
            RuleFor(x => x.Ppe.Count).LessThanOrEqualTo(MaxRows).WithName("ppe")
                .WithMessage($"Не більше {MaxRows} ЗІЗ за один імпорт.");
            RuleForEach(x => x.Sewing).NotNull().ChildRules(row =>
            {
                row.RuleFor(x => x.Articles).NotNull().WithMessage("У моделі немає списку артикулів.");
                row.RuleFor(x => x.Materials).NotNull().WithMessage("У моделі немає списку матеріалів.");
                row.RuleFor(x => x.Operations).NotNull().WithMessage("У моделі немає списку операцій.");
            });
            RuleForEach(x => x.Ppe).NotNull();
        });
    }
}
