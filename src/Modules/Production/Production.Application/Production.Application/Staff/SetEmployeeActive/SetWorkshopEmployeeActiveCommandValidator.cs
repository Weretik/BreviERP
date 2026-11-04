using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.SetEmployeeActive;

public sealed class SetWorkshopEmployeeActiveCommandValidator : AbstractValidator<SetWorkshopEmployeeActiveCommand>
{
    public SetWorkshopEmployeeActiveCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
