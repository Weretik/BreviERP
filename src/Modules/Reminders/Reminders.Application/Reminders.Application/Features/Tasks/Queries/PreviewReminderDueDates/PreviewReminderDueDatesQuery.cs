using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Queries.PreviewReminderDueDates;

/// <summary>Next deadlines for an unsaved rule, shown live in the task form.</summary>
public sealed record PreviewReminderDueDatesQuery(ReminderScheduleInput Schedule, DateOnly? From)
    : IQuery<Result<IReadOnlyList<DueDatePreview>>>;
