using Production.Application.Features.Timesheet.Shared.Month;

namespace Production.Application.Features.Timesheet.Shared;

/// <summary>«Робочий час» of a day: every worker with the minutes marked, the changes of the day and the month.</summary>
public sealed record TimesheetDayResponse(
    DateOnly Date,
    bool CanEdit,
    bool IsMonthClosed,
    IReadOnlyList<TimesheetRowResponse> Rows,
    IReadOnlyList<TimesheetChangeResponse> Changes,
    TimesheetMonthResponse Month);
