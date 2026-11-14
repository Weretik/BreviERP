namespace Reminders.Application.Features.Calendar.Shared;

public sealed record NonWorkingDayResponse(DateOnly Date, string? Note);
