using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Queries.GetPayrollMonth;

public sealed class GetPayrollMonthHandler(PayrollChanges changes)
    : IQueryHandler<GetPayrollMonthQuery, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(GetPayrollMonthQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesPayroll) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotSee);
        PayrollMonths.TryParse(query.Month, out var month);
        return Result.Success(await changes.ReadAsync(query.Actor, month, cancellationToken));
    }
}
