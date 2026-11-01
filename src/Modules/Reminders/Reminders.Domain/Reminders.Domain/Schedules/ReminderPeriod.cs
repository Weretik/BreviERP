namespace Reminders.Domain.Schedules;

/// <summary>A reporting period: one calendar month, quarter, or year.</summary>
public readonly record struct ReminderPeriod(ReminderPeriodicity Periodicity, DateOnly Start)
{
    private static readonly string[] MonthNames =
        ["січень", "лютий", "березень", "квітень", "травень", "червень",
         "липень", "серпень", "вересень", "жовтень", "листопад", "грудень"];

    private int Months => Periodicity switch
    {
        ReminderPeriodicity.Monthly => 1,
        ReminderPeriodicity.Quarterly => 3,
        ReminderPeriodicity.Yearly => 12,
        _ => throw new ArgumentOutOfRangeException(nameof(Periodicity))
    };

    public DateOnly End => Start.AddMonths(Months).AddDays(-1);

    /// <summary>The first day of the period's last month.</summary>
    public DateOnly LastMonth => Start.AddMonths(Months - 1);

    public ReminderPeriod Next() => this with { Start = Start.AddMonths(Months) };

    public static ReminderPeriod Containing(ReminderPeriodicity periodicity, DateOnly date)
    {
        var month = periodicity switch
        {
            ReminderPeriodicity.Monthly => date.Month,
            ReminderPeriodicity.Quarterly => (date.Month - 1) / 3 * 3 + 1,
            ReminderPeriodicity.Yearly => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(periodicity))
        };
        return new ReminderPeriod(periodicity, new DateOnly(date.Year, month, 1));
    }

    /// <summary>Ukrainian label shown in messages: "жовтень 2026", "III квартал 2026", "2026 рік".</summary>
    public string Label => Periodicity switch
    {
        ReminderPeriodicity.Monthly => $"{MonthNames[Start.Month - 1]} {Start.Year}",
        ReminderPeriodicity.Quarterly => $"{new[] { "I", "II", "III", "IV" }[(Start.Month - 1) / 3]} квартал {Start.Year}",
        _ => $"{Start.Year} рік"
    };
}
