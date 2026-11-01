namespace Reminders.Domain.Schedules;

public enum DueRuleKind
{
    /// <summary>Day N of the month that is MonthOffset months after the period's last month.</summary>
    DayOfMonth = 1,

    /// <summary>The last working day of the month that is MonthOffset months after the period's last month.</summary>
    LastWorkingDayOfMonth = 2,

    /// <summary>N calendar days after the last day of the period.</summary>
    DaysAfterPeriodEnd = 3
}
