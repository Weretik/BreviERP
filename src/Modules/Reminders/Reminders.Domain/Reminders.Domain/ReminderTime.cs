namespace Reminders.Domain;

/// <summary>Reminders follow the business calendar in Kyiv, whatever the server time zone is.</summary>
public static class ReminderTime
{
    public static TimeZoneInfo Kyiv { get; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateTimeOffset ToKyiv(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, Kyiv);

    public static DateOnly TodayInKyiv(DateTimeOffset now) => DateOnly.FromDateTime(ToKyiv(now).DateTime);
}
