namespace Scheduling.Domain.Schedules;

/// <summary>Rules a schedule breaks; messages are shown to the admin as they are.</summary>
public static class ScheduleErrors
{
    public const int MinIntervalMinutes = 1;
    public const int MaxIntervalMinutes = 1440;
    public const int MaxTimes = 6;

    public static Error IntervalOutOfRange { get; } = Error.Validation("Scheduling.Schedule.Interval",
        $"Інтервал має бути від {MinIntervalMinutes} до {MaxIntervalMinutes} хвилин.", "schedule.everyMinutes");

    public static Error WindowInvalid { get; } = Error.Validation("Scheduling.Schedule.Window",
        "Години роботи: «з» і «до» від 0 до 23, «з» менше за «до».", "schedule.fromHour");

    public static Error TimesInvalid { get; } = Error.Validation("Scheduling.Schedule.Times",
        $"Вкажіть від 1 до {MaxTimes} різних часів запуску.", "schedule.times");

    public static Error DaysInvalid { get; } = Error.Validation("Scheduling.Schedule.Days",
        "Оберіть хоча б один день тижня.", "schedule.days");
}
