namespace Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;

public sealed record TimesheetEntryInput(int EmployeeId, int Minutes, int? Version);
