using Production.Application.Abstractions;

namespace Production.Application.Features.Staff.Commands.SetWorkshopEmployeeActive;

public sealed class SetWorkshopEmployeeActiveValidator : AbstractValidator<SetWorkshopEmployeeActiveCommand>
{
    public SetWorkshopEmployeeActiveValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
