
namespace Reminders.Application.Features.Occurrences.Queries.GetReminderOccurrences;

/// <summary>Current and past periods, newest deadline first. Status: open, done, or null for all.</summary>
public sealed record GetReminderOccurrencesQuery(string? Status, long? TaskId, int Page, int PageSize)
    : IQuery<Result<ReminderOccurrencePage>>;
