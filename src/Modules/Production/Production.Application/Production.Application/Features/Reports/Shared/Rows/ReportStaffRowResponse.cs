namespace Production.Application.Features.Reports.Shared.Rows;

/// <summary>A worker over the months; <c>DayAverage</c> is the pay per 8 hours of «Робочий час», null without hours.</summary>
public sealed record ReportStaffRowResponse(int EmployeeId, string Name, string Position, int Minutes, int Sewn,
    int Cut, decimal Accrued, decimal? DayAverage);
