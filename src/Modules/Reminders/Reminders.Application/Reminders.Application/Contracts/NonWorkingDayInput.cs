namespace Reminders.Application.Contracts;

public sealed record NonWorkingDayInput(DateOnly Date, string? Note);
