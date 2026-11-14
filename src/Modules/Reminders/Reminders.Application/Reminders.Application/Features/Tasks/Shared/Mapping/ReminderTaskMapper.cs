using Reminders.Domain.Calendar;
using Reminders.Domain.Chats;
using Reminders.Domain.Schedules;
using Reminders.Domain.Tasks;
using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Shared.Mapping;

public static class ReminderTaskMapper
{
    public static ReminderSchedule ToSchedule(ReminderScheduleInput input) => new(
        ReminderCodes.Periodicities[input.Periodicity],
        new DueRule(ReminderCodes.DueRules[input.DueRule], input.Day, input.MonthOffset),
        input.LeadDays);

    public static ReminderScheduleInput ToInput(ReminderSchedule schedule) => new(
        ReminderCodes.Code(ReminderCodes.Periodicities, schedule.Periodicity),
        ReminderCodes.Code(ReminderCodes.DueRules, schedule.Due.Kind),
        schedule.Due.Day, schedule.Due.MonthOffset, schedule.LeadDays);

    /// <summary>The next deadlines from a date, so Admin can see the rule does what was meant.</summary>
    public static IReadOnlyList<DueDatePreview> Preview(ReminderSchedule schedule, DateOnly from, WorkingCalendar calendar, int count = 3) =>
        schedule.PeriodsDueBetween(from, from.AddYears(3), calendar).Take(count)
            .Select(x => new DueDatePreview(x.Period.Label, x.DueDate, schedule.WindowStart(x.DueDate))).ToArray();

    public static ReminderTaskResponse ToResponse(ReminderTask task, ReminderChat? chat, DateOnly today, WorkingCalendar calendar) => new(
        task.Id.Value,
        task.Title,
        ReminderCodes.Code(ReminderCodes.Organizations, task.Organization),
        ToInput(task.Schedule),
        task.ChatId?.Value,
        chat?.Name,
        task.ActiveFrom,
        task.IsEnabled,
        Preview(task.Schedule, today > task.ActiveFrom ? today : task.ActiveFrom, calendar),
        task.CreatedAt,
        task.UpdatedAt);
}
