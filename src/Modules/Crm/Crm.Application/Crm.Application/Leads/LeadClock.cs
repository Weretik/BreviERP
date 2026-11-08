namespace Crm.Application.Leads;

/// <summary>«Сьогодні» of «Мій день» and the statistics periods follow the Kyiv calendar day.</summary>
internal static class LeadClock
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateOnly KyivDate(DateTimeOffset utc) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(utc, Kyiv).DateTime);

    /// <summary>The UTC instant a Kyiv calendar day starts.</summary>
    public static DateTimeOffset StartOf(DateOnly kyivDate)
    {
        var local = kyivDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Kyiv.GetUtcOffset(local)).ToUniversalTime();
    }

    public static DateTimeOffset EndOfToday(DateTimeOffset now) => StartOf(KyivDate(now).AddDays(1));

    public static DateTimeOffset StartOfToday(DateTimeOffset now) => StartOf(KyivDate(now));
}
