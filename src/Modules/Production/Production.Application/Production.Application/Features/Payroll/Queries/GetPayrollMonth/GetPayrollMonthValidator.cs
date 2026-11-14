using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Queries.GetPayrollMonth;

public sealed class GetPayrollMonthValidator : AbstractValidator<GetPayrollMonthQuery>
{
    public GetPayrollMonthValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
    }
}
