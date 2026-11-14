namespace Production.Application.Features.Timesheet.Shared.Month;

public sealed record TimesheetMonthDayResponse(DateOnly Date, int People, int Minutes);
