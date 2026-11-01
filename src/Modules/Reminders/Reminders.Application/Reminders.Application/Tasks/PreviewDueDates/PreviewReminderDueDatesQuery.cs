using Reminders.Application.Contracts;

namespace Reminders.Application.Tasks.PreviewDueDates;

/// <summary>Next deadlines for an unsaved rule, shown live in the task form.</summary>
public sealed record PreviewReminderDueDatesQuery(ReminderScheduleInput Schedule, DateOnly? From)
    : IQuery<Result<IReadOnlyList<DueDatePreview>>>;
