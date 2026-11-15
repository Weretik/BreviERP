using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

public sealed class GetPayrollYearHandler(PayrollSheets sheets, TimeProvider clock)
    : IQueryHandler<GetPayrollYearQuery, Result<PayrollYearResponse>>
{
    public async ValueTask<Result<PayrollYearResponse>> Handle(GetPayrollYearQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesPayroll) return Result<PayrollYearResponse>.Forbidden(PayrollMessages.CannotSee);
        var current = PayrollMonths.Current(clock);
        var first = new DateOnly(query.Year, 1, 1);
        var last = current.Year == query.Year ? current : new DateOnly(query.Year, 12, 1);
        List<PayrollMonthResponse> months = first > current
            ? []
            : (await sheets.ReadRangeAsync(first, last, cancellationToken)).Select(x => x.Month).ToList();

        var rows = months.SelectMany((month, index) => month.Lines.Select(line => (index, line)))
            .Where(x => x.line.Accrued != 0)
            .GroupBy(x => x.line.EmployeeId)
            .Select(group =>
            {
                var last = group.Last().line;
                var accrued = new decimal?[12];
                foreach (var (index, line) in group) accrued[index] = line.Accrued;
                return new PayrollYearRowResponse(group.Key, last.Name, last.Position, accrued,
                    group.Sum(x => x.line.Accrued));
            })
            .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        return Result.Success(new PayrollYearResponse(query.Year,
            [.. months.Select(x => new PayrollYearMonthResponse(x.Month, x.IsClosed, x.Totals.Accrued))], rows));
    }
}
