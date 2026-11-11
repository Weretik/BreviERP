using System.Globalization;
using System.Text;

namespace Warehouse.Domain.Stock;

/// <summary>
/// A fabric colour as staff type it. The key is Unicode NFC, trimmed, inner spaces collapsed and lower case (uk-UA), so
/// «  Білий » and «білий» meet; it never joins different shades.
/// </summary>
public static class StockColor
{
    public const int MaxLength = 80;

    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    public static string Clean(string? color) => WarehouseText.Clean(color?.Normalize(NormalizationForm.FormC), MaxLength);

    public static string Key(string color)
    {
        var text = string.Join(' ', (color ?? string.Empty).Normalize(NormalizationForm.FormC)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return text.ToLower(Ukrainian);
    }
}
