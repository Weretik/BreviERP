using Production.Application.Abstractions;
using Production.Domain.Staff;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Commands.CreateWorkshopEmployee;

public sealed class CreateWorkshopEmployeeValidator : AbstractValidator<CreateWorkshopEmployeeCommand>
{
    public CreateWorkshopEmployeeValidator() =>
        RuleFor(x => x.Input).NotNull().SetValidator(new EmployeeInputValidator());
}
