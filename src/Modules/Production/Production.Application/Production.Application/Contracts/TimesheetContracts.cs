namespace Production.Application.Contracts;

/// <summary>«Робочий час» of a day: every worker with the minutes marked, the changes of the day and the month.</summary>
public sealed record TimesheetDayResponse(
    DateOnly Date,
    bool CanEdit,
    bool IsMonthClosed,
    IReadOnlyList<TimesheetRowResponse> Rows,
    IReadOnlyList<TimesheetChangeResponse> Changes,
    TimesheetMonthResponse Month);

/// <summary>A worker on the day; <c>Minutes</c> is null while the day is not marked for them.</summary>
public sealed record TimesheetRowResponse(int EmployeeId, string Name, string Position, bool IsActive, int? Minutes,
    int? Version);

/// <summary><c>BeforeMinutes</c> is null on the first mark.</summary>
public sealed record TimesheetChangeResponse(int Id, int EmployeeId, string EmployeeName, int? BeforeMinutes,
    int AfterMinutes, DateTimeOffset ChangedAt, string ChangedByName);

/// <summary>«Архів табеля»: totals of the month by day and by worker.</summary>
public sealed record TimesheetMonthResponse(string Month, IReadOnlyList<TimesheetMonthDayResponse> Days,
    IReadOnlyList<TimesheetMonthEmployeeResponse> Employees);

public sealed record TimesheetMonthDayResponse(DateOnly Date, int People, int Minutes);

public sealed record TimesheetMonthEmployeeResponse(int EmployeeId, string Name, string Position, int Days, int Minutes);

/// <summary>Saves the marks of a day; <c>Version</c> is the one read, null for a first mark.</summary>
public sealed record SaveTimesheetDayInput(DateOnly Date, IReadOnlyList<TimesheetEntryInput> Entries);

public sealed record TimesheetEntryInput(int EmployeeId, int Minutes, int? Version);
