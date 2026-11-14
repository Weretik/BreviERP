using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Queries.GetPayrollEmployee;

public sealed class GetPayrollEmployeeValidator : AbstractValidator<GetPayrollEmployeeQuery>
{
    public GetPayrollEmployeeValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}
