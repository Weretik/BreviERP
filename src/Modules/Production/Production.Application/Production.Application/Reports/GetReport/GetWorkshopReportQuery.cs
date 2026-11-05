using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Application.Payroll;

namespace Production.Application.Reports.GetReport;

/// <summary>
/// «Звіти цеху» of the months <c>From</c>..<c>To</c> («РРРР-ММ», at most 12, not in the future) and, when both are
/// given, of the months to compare with (PRD-D48).
/// </summary>
public sealed record GetWorkshopReportQuery(ProductionActor Actor, string From, string To, string? CompareFrom,
    string? CompareTo) : IQuery<Result<WorkshopReportPairResponse>>;

public sealed class GetWorkshopReportQueryValidator : AbstractValidator<GetWorkshopReportQuery>
{
    public GetWorkshopReportQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x).Custom((query, context) =>
        {
            Check(query.From, query.To, nameof(query.From), context);
            if (query.CompareFrom is null && query.CompareTo is null) return;
            if (query.CompareFrom is null || query.CompareTo is null)
                context.AddFailure(nameof(query.CompareFrom), ReportMessages.Comparison);
            else Check(query.CompareFrom, query.CompareTo, nameof(query.CompareFrom), context);
        });
    }

    private static void Check(string fromText, string toText, string property,
        ValidationContext<GetWorkshopReportQuery> context)
    {
        if (!PayrollMonths.TryParse(fromText, out var from) || !PayrollMonths.TryParse(toText, out var to))
            context.AddFailure(property, ReportMessages.Month);
        else if (from > to) context.AddFailure(property, ReportMessages.Range);
        else if (Months(from, to) > ReportMessages.MaxMonths) context.AddFailure(property, ReportMessages.TooLong);
    }

    private static int Months(DateOnly from, DateOnly to) => (to.Year - from.Year) * 12 + to.Month - from.Month + 1;
}

public sealed class GetWorkshopReportQueryHandler(WorkshopReports reports, TimeProvider clock)
    : IQueryHandler<GetWorkshopReportQuery, Result<WorkshopReportPairResponse>>
{
    public async ValueTask<Result<WorkshopReportPairResponse>> Handle(GetWorkshopReportQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.SeesReports) return Result<WorkshopReportPairResponse>.Forbidden(ReportMessages.CannotSee);
        var current = PayrollMonths.Format(PayrollMonths.Current(clock));
        if (string.CompareOrdinal(query.To, current) > 0 ||
            query.CompareTo is { } compareTo && string.CompareOrdinal(compareTo, current) > 0)
            return Result<WorkshopReportPairResponse>.Invalid(new ValidationError(nameof(query.To), ReportMessages.Future));
        var report = await ReadAsync(query.From, query.To, cancellationToken);
        var comparison = query.CompareFrom is { } from && query.CompareTo is { } to
            ? await ReadAsync(from, to, cancellationToken)
            : null;
        return Result.Success(new WorkshopReportPairResponse(report, comparison));
    }

    private Task<WorkshopReportResponse> ReadAsync(string from, string to, CancellationToken cancellationToken)
    {
        PayrollMonths.TryParse(from, out var first);
        PayrollMonths.TryParse(to, out var last);
        return reports.ReadAsync(first, last, cancellationToken);
    }
}
