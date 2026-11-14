namespace Production.Application.Features.Timesheet.Shared.Month;

/// <summary>A worker on the day; <c>Minutes</c> is null while the day is not marked for them.</summary>
public sealed record TimesheetRowResponse(int EmployeeId, string Name, string Position, bool IsActive, int? Minutes,
    int? Version);
