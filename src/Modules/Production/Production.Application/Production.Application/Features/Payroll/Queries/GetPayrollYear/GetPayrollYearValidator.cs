using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

public sealed class GetPayrollYearValidator : AbstractValidator<GetPayrollYearQuery>
{
    public GetPayrollYearValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Year).InclusiveBetween(2020, 2100).WithMessage(PayrollMessages.Year);
    }
}
