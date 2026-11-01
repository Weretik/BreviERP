using Reminders.Domain.Calendar;

namespace Reminders.Domain.Schedules;

/// <summary>How a task's deadline is derived from its period. Day numbers above the month length use the last day.</summary>
public sealed record DueRule(DueRuleKind Kind, int Day, int MonthOffset)
{
    public const int MaxMonthOffset = 12;

    private DueRule() : this(DueRuleKind.DayOfMonth, 1, 0) { }
    public const int MaxDaysAfterPeriodEnd = 366;

    public static DueRule DayOfMonth(int day, int monthOffset) => new(DueRuleKind.DayOfMonth, day, monthOffset);
    public static DueRule LastWorkingDay(int monthOffset) => new(DueRuleKind.LastWorkingDayOfMonth, 0, monthOffset);
    public static DueRule DaysAfterEnd(int days) => new(DueRuleKind.DaysAfterPeriodEnd, days, 0);

    public static IReadOnlyList<string> Validate(DueRuleKind kind, int day, int monthOffset)
    {
        var errors = new List<string>();
        switch (kind)
        {
            case DueRuleKind.DayOfMonth:
                if (day is < 1 or > 31) errors.Add("Day must be between 1 and 31.");
                if (monthOffset is < 0 or > MaxMonthOffset) errors.Add($"MonthOffset must be between 0 and {MaxMonthOffset}.");
                break;
            case DueRuleKind.LastWorkingDayOfMonth:
                if (monthOffset is < 0 or > MaxMonthOffset) errors.Add($"MonthOffset must be between 0 and {MaxMonthOffset}.");
                break;
            case DueRuleKind.DaysAfterPeriodEnd:
                if (day is < 1 or > MaxDaysAfterPeriodEnd) errors.Add($"Day must be between 1 and {MaxDaysAfterPeriodEnd}.");
                break;
            default:
                errors.Add("Unknown due rule kind.");
                break;
        }
        return errors;
    }

    public DateOnly DueDate(ReminderPeriod period, WorkingCalendar calendar)
    {
        var month = period.LastMonth.AddMonths(MonthOffset);
        return Kind switch
        {
            DueRuleKind.DayOfMonth => new DateOnly(month.Year, month.Month,
                Math.Min(Day, DateTime.DaysInMonth(month.Year, month.Month))),
            DueRuleKind.LastWorkingDayOfMonth => calendar.LastWorkingDayOfMonth(month.Year, month.Month),
            DueRuleKind.DaysAfterPeriodEnd => period.End.AddDays(Day),
            _ => throw new InvalidOperationException("Unknown due rule kind.")
        };
    }
}
