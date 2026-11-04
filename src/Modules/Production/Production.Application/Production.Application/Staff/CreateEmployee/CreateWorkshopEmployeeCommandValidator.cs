using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.Staff;

namespace Production.Application.Staff.CreateEmployee;

public sealed class CreateWorkshopEmployeeCommandValidator : AbstractValidator<CreateWorkshopEmployeeCommand>
{
    public CreateWorkshopEmployeeCommandValidator() =>
        RuleFor(x => x.Input).NotNull().SetValidator(new EmployeeInputValidator());
}
