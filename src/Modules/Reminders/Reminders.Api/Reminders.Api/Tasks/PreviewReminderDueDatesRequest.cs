using Reminders.Application.Contracts;

namespace Reminders.Api.Tasks;

public sealed record PreviewReminderDueDatesRequest(ReminderScheduleInput Schedule, DateOnly? From);
