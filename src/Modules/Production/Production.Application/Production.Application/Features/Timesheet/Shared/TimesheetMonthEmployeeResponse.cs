namespace Production.Application.Features.Timesheet.Shared;

public sealed record TimesheetMonthEmployeeResponse(int EmployeeId, string Name, string Position, int Days, int Minutes);
