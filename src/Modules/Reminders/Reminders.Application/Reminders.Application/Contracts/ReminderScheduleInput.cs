namespace Reminders.Application.Contracts;

public sealed record ReminderScheduleInput(string Periodicity, string DueRule, int Day, int MonthOffset, int LeadDays);
