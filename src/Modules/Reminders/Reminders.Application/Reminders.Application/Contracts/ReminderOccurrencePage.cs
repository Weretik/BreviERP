namespace Reminders.Application.Contracts;

public sealed record ReminderOccurrencePage(IReadOnlyList<ReminderOccurrenceResponse> Items, int Total);
