namespace Production.Application.Features.Timesheet.Shared;

public sealed record TimesheetMonthDayResponse(DateOnly Date, int People, int Minutes);
