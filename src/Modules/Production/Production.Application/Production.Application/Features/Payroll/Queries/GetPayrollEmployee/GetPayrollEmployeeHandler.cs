using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Queries.GetPayrollEmployee;

public sealed class GetPayrollEmployeeHandler(PayrollSheets sheets)
    : IQueryHandler<GetPayrollEmployeeQuery, Result<PayrollEmployeeResponse>>
{
    public async ValueTask<Result<PayrollEmployeeResponse>> Handle(GetPayrollEmployeeQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesPayroll) return Result<PayrollEmployeeResponse>.Forbidden(PayrollMessages.CannotSee);
        PayrollMonths.TryParse(query.Month, out var month);
        var snapshot = await sheets.ReadAsync(month, cancellationToken);
        var line = snapshot.Month.Lines.FirstOrDefault(x => x.EmployeeId == query.EmployeeId);
        if (line is null) return Result<PayrollEmployeeResponse>.NotFound(PayrollMessages.EmployeeNotFound);
        return Result.Success(new PayrollEmployeeResponse(snapshot.Month.Month, snapshot.Month.IsClosed, line,
            [.. snapshot.Work.Where(x => x.EmployeeId == query.EmployeeId)],
            [.. snapshot.Month.Payments.Where(x => x.EmployeeId == query.EmployeeId)],
            [.. snapshot.Month.Adjustments.Where(x => x.EmployeeId == query.EmployeeId)]));
    }
}
