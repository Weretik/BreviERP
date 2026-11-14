namespace Reminders.Application.Features.Tasks.Shared;

public sealed record ReminderScheduleInput(string Periodicity, string DueRule, int Day, int MonthOffset, int LeadDays);
