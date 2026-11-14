using Production.Application.Abstractions;
using Production.Domain;
using Production.Domain.Staff;

namespace Production.Application.Features.Staff.Commands.ImportWorkshopStaff;

public sealed class ImportWorkshopStaffValidator : AbstractValidator<ImportWorkshopStaffCommand>
{
    public const int MaxRows = 500;

    public ImportWorkshopStaffValidator()
    {
        RuleFor(x => x.Rows).NotEmpty().Must(x => x is null || x.Count <= MaxRows)
            .WithMessage($"Не більше {MaxRows} працівників за раз.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.FullName).NotEmpty().MaximumLength(WorkshopEmployee.FullNameMaxLength);
            row.RuleFor(x => x.Position).NotEmpty().MaximumLength(WorkshopEmployee.PositionMaxLength);
        });
        RuleFor(x => x.Rows).Must(x => x is null || x.Where(r => !string.IsNullOrWhiteSpace(r.FullName))
                .GroupBy(r => ProductionText.Key(r.FullName)).All(g => g.Count() == 1))
            .WithMessage("У файлі є однакові ПІБ.");
    }
}
