namespace Reminders.Application.Features.Tasks.Shared;

public sealed record ReminderTaskResponse(
    long Id,
    string Title,
    string Organization,
    ReminderScheduleInput Schedule,
    long? ChatId,
    string? ChatName,
    DateOnly ActiveFrom,
    bool IsEnabled,
    IReadOnlyList<DueDatePreview> NextDueDates,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
