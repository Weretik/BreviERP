using System.Globalization;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>The import date (dd.MM.yyyy, Kyiv) that a new PPE offer version gets as its source date.</summary>
public static class SheetDates
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Kyiv).DateTime);

    public static string Format(DateOnly date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
