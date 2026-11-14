using Reminders.Application.Features.Occurrences.Shared;
namespace Reminders.Application.Features.Occurrences.Queries.GetReminderOccurrences;

public sealed record ReminderOccurrencePage(IReadOnlyList<ReminderOccurrenceResponse> Items, int Total);
