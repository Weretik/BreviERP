namespace Production.Application.Features.Timesheet.Shared;

/// <summary><c>BeforeMinutes</c> is null on the first mark.</summary>
public sealed record TimesheetChangeResponse(int Id, int EmployeeId, string EmployeeName, int? BeforeMinutes,
    int AfterMinutes, DateTimeOffset ChangedAt, string ChangedByName);
