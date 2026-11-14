using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;

public sealed class SetPayrollEmbroideryValidator : AbstractValidator<SetPayrollEmbroideryCommand>
{
    public SetPayrollEmbroideryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Stitches).InclusiveBetween(0, PayrollMonth.MaxStitches).WithMessage(PayrollMessages.Stitches)
            .When(x => x.Input is not null);
        RuleFor(x => x.Input.EmployeeId).GreaterThan(0).When(x => x.Input?.EmployeeId is not null);
    }
}
