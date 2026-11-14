namespace Production.Application.Features.Reports.Shared;

/// <summary>
/// The report of the months <c>From</c>..<c>To</c> («РРРР-ММ»). <c>ClosedMonths</c> are read as they were closed in
/// «Зарплата»; the others are counted from the current data and can still change.
/// </summary>
public sealed record WorkshopReportResponse(
    string From,
    string To,
    IReadOnlyList<string> ClosedMonths,
    IReadOnlyList<string> OpenMonths,
    WorkshopReportTotalsResponse Totals,
    IReadOnlyList<ReportOutputRowResponse> Output,
    IReadOnlyList<ReportStaffRowResponse> Staff,
    IReadOnlyList<ReportIssueRowResponse> Materials,
    IReadOnlyList<ReportStockRowResponse> Stock,
    IReadOnlyList<string> Warnings);
