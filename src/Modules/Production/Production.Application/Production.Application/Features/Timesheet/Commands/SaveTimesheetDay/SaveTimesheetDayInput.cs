namespace Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;

/// <summary>Saves the marks of a day; <c>Version</c> is the one read, null for a first mark.</summary>
public sealed record SaveTimesheetDayInput(DateOnly Date, IReadOnlyList<TimesheetEntryInput> Entries);
