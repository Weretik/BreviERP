using BuildingBlocks.Domain.Errors;

namespace Reminders.Domain.Tasks;

public static class ReminderTaskErrors
{
    public static Error TitleInvalid() => Error.Validation("Reminders.Task.Title.Invalid",
        $"Вкажіть назву нагадування до {ReminderTask.TitleMaxLength} символів.", "title");

    public static Error LeadDaysOutOfRange() => Error.Validation("Reminders.Task.LeadDays.OutOfRange",
        $"Нагадувати можна за 0–{ReminderSchedule.MaxLeadDays} днів.", "schedule.leadDays");
}
