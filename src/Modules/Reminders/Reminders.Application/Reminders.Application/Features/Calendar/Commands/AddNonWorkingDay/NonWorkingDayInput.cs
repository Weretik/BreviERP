namespace Reminders.Application.Features.Calendar.Commands.AddNonWorkingDay;

public sealed record NonWorkingDayInput(DateOnly Date, string? Note);
