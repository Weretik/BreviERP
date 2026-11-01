namespace Reminders.Domain.Calendar;

/// <summary>Working days are Monday to Friday minus the configured extra non-working days.</summary>
public sealed class WorkingCalendar(IEnumerable<DateOnly> nonWorkingDays)
{
    private readonly HashSet<DateOnly> _nonWorkingDays = [.. nonWorkingDays];

    public static WorkingCalendar WeekendsOnly { get; } = new([]);

    public bool IsWorkingDay(DateOnly date) =>
        date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !_nonWorkingDays.Contains(date);

    public DateOnly LastWorkingDayOnOrBefore(DateOnly date)
    {
        var day = date;
        while (!IsWorkingDay(day)) day = day.AddDays(-1);
        return day;
    }

    public DateOnly NextWorkingDayAfter(DateOnly date)
    {
        var day = date.AddDays(1);
        while (!IsWorkingDay(day)) day = day.AddDays(1);
        return day;
    }

    public DateOnly AddWorkingDays(DateOnly date, int count)
    {
        var day = date;
        for (var i = 0; i < count; i++) day = NextWorkingDayAfter(day);
        return day;
    }

    public DateOnly LastWorkingDayOfMonth(int year, int month) =>
        LastWorkingDayOnOrBefore(new DateOnly(year, month, DateTime.DaysInMonth(year, month)));
}
