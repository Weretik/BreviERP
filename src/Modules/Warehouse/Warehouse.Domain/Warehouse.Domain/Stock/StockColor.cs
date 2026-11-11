namespace Warehouse.Domain.Stock;

/// <summary>A fabric colour as staff type it; the key ignores case and extra spaces so «Темно-синій» and «темно-синій » meet.</summary>
public static class StockColor
{
    public const int MaxLength = 80;

    public static string Clean(string? color) => WarehouseText.Clean(color, MaxLength);

    public static string Key(string color) => WarehouseText.Key(color);
}
