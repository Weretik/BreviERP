using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Reports.Shared;

namespace Production.Application.Features.Reports.Queries.GetWorkshopReport;

public sealed class GetWorkshopReportHandler(WorkshopReports reports, TimeProvider clock)
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
