using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;

namespace Production.Application.Payroll.GetMonth;

/// <summary>«Зарплата» of a month «РРРР-ММ».</summary>
public sealed record GetPayrollMonthQuery(ProductionActor Actor, string Month) : IQuery<Result<PayrollMonthResponse>>;

public sealed class GetPayrollMonthQueryValidator : AbstractValidator<GetPayrollMonthQuery>
{
    public GetPayrollMonthQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
    }
}

public sealed class GetPayrollMonthQueryHandler(PayrollChanges changes)
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

/// <summary>A worker's month in detail.</summary>
public sealed record GetPayrollEmployeeQuery(ProductionActor Actor, string Month, int EmployeeId)
    : IQuery<Result<PayrollEmployeeResponse>>;

public sealed class GetPayrollEmployeeQueryValidator : AbstractValidator<GetPayrollEmployeeQuery>
{
    public GetPayrollEmployeeQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
        RuleFor(x => x.EmployeeId).GreaterThan(0);
    }
}

public sealed class GetPayrollEmployeeQueryHandler(PayrollSheets sheets)
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

/// <summary>«Історія за рік»: what each worker earned per month, up to the current month.</summary>
public sealed record GetPayrollYearQuery(ProductionActor Actor, int Year) : IQuery<Result<PayrollYearResponse>>;

public sealed class GetPayrollYearQueryValidator : AbstractValidator<GetPayrollYearQuery>
{
    public GetPayrollYearQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Year).InclusiveBetween(2020, 2100).WithMessage(PayrollMessages.Year);
    }
}

public sealed class GetPayrollYearQueryHandler(PayrollSheets sheets, TimeProvider clock)
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
