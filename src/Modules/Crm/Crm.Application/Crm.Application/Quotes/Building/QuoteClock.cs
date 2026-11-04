namespace Crm.Application.Quotes.Building;

/// <summary>КП numbers follow the Kyiv calendar month.</summary>
internal static class QuoteClock
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateTime KyivDate(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, Kyiv).DateTime;
}
