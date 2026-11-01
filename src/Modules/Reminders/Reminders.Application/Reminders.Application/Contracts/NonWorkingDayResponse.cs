namespace Reminders.Application.Contracts;

public sealed record NonWorkingDayResponse(DateOnly Date, string? Note);
