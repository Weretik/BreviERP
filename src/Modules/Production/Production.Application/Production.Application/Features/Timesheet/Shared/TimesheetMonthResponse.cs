namespace Production.Application.Features.Timesheet.Shared;

/// <summary>«Архів табеля»: totals of the month by day and by worker.</summary>
public sealed record TimesheetMonthResponse(string Month, IReadOnlyList<TimesheetMonthDayResponse> Days,
    IReadOnlyList<TimesheetMonthEmployeeResponse> Employees);
