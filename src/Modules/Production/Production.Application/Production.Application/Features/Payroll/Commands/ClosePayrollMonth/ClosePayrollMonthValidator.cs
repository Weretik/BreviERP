using Production.Domain.Payroll;
using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.ClosePayrollMonth;

public sealed class ClosePayrollMonthValidator : AbstractValidator<ClosePayrollMonthCommand>
{
    public ClosePayrollMonthValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
    }
}
