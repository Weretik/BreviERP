using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.UpdateEmployee;

public sealed class UpdateWorkshopEmployeeCommandValidator : AbstractValidator<UpdateWorkshopEmployeeCommand>
{
    public UpdateWorkshopEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new EmployeeInputValidator());
    }
}
