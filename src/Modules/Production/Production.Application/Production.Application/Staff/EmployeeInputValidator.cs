using Production.Application.Contracts;
using Production.Domain.Staff;

namespace Production.Application.Staff;

internal sealed class EmployeeInputValidator : AbstractValidator<WorkshopEmployeeInput>
{
    public EmployeeInputValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Вкажіть ПІБ працівника.")
            .Must(x => x is null || x.Trim().Length <= WorkshopEmployee.FullNameMaxLength)
            .WithMessage($"ПІБ не довше {WorkshopEmployee.FullNameMaxLength} символів.");
        RuleFor(x => x.Position).NotEmpty().WithMessage("Вкажіть посаду.")
            .Must(x => x is null || x.Trim().Length <= WorkshopEmployee.PositionMaxLength)
            .WithMessage($"Посада не довше {WorkshopEmployee.PositionMaxLength} символів.");
    }
}
