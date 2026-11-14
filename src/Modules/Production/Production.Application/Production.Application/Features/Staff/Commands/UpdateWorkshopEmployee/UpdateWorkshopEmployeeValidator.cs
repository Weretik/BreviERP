using Production.Application.Abstractions;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Commands.UpdateWorkshopEmployee;

public sealed class UpdateWorkshopEmployeeValidator : AbstractValidator<UpdateWorkshopEmployeeCommand>
{
    public UpdateWorkshopEmployeeValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new EmployeeInputValidator());
    }
}
