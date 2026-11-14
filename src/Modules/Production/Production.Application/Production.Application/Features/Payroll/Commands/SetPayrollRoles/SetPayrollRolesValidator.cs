using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.SetPayrollRoles;

public sealed class SetPayrollRolesValidator : AbstractValidator<SetPayrollRolesCommand>
{
    public SetPayrollRolesValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.MasterEmployeeId).GreaterThan(0).When(x => x.Input?.MasterEmployeeId is not null);
        RuleFor(x => x.Input.ChiefEmployeeId).GreaterThan(0).When(x => x.Input?.ChiefEmployeeId is not null);
    }
}
