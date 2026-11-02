namespace Reference.Application.Features.Shared.Dates;

/// <summary>Today's date in Kyiv, used as «Дата оновлення ціни» when a price is edited in Brevi.</summary>
internal static class KyivDate
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateOnly Today(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), Kyiv).DateTime);
}
