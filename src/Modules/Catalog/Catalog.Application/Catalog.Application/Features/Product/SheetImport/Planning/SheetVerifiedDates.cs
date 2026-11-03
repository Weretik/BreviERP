using System.Globalization;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>The sheet check date (dd.MM.yyyy) and its Kyiv midnight.</summary>
public static class SheetVerifiedDates
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static bool TryParse(string? value, out DateOnly date)
        => DateOnly.TryParseExact(value?.Trim(), Catalog.Domain.Products.Entities.Product.VerifiedOnFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    public static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Kyiv).DateTime);

    /// <summary>Kyiv midnight as a UTC instant: PostgreSQL <c>timestamptz</c> accepts only offset 0.</summary>
    public static DateTimeOffset KyivMidnight(DateOnly date)
    {
        var midnight = date.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(midnight, Kyiv.GetUtcOffset(midnight)).ToUniversalTime();
    }

    public static string Format(DateOnly date)
        => date.ToString(Catalog.Domain.Products.Entities.Product.VerifiedOnFormat, CultureInfo.InvariantCulture);
}
