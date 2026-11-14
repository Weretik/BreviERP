using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Api.Tasks;

public sealed record PreviewReminderDueDatesRequest(ReminderScheduleInput Schedule, DateOnly? From);
