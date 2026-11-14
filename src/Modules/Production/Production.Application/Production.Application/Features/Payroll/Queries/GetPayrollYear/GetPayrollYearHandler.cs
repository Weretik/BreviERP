using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

public sealed class GetPayrollYearHandler(PayrollSheets sheets, TimeProvider clock)
    : IQueryHandler<GetPayrollYearQuery, Result<PayrollYearResponse>>
{
    public async ValueTask<Result<PayrollYearResponse>> Handle(GetPayrollYearQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesPayroll) return Result<PayrollYearResponse>.Forbidden(PayrollMessages.CannotSee);
        var current = PayrollMonths.Current(clock);
        var months = new List<PayrollMonthResponse>();
        for (var m = new DateOnly(query.Year, 1, 1); m <= current && m.Year == query.Year; m = m.AddMonths(1))
            months.Add((await sheets.ReadAsync(m, cancellationToken)).Month);

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
