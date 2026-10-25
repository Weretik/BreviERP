using System.Globalization;
using System.Text.Json;

namespace CatalogImport.Source;

public sealed record WorkbookRow(string Sheet, int Number, JsonElement[] Cells)
{
    public string Text(int column)
    {
        if (column >= Cells.Length || Cells[column].ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return string.Empty;
        return Cells[column].ToString().Trim();
    }

    public decimal? Decimal(int column)
    {
        var value = Text(column);
        if (value.Length == 0)
            return null;
        if (decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var result))
            return result;
        throw new InvalidDataException($"{Sheet}!{Number}: invalid numeric column {column + 1}.");
    }

    public bool Flag(int column)
    {
        var value = Text(column);
        return value is "True" or "true" or "Так" or "Да" or "1";
    }

    public string Key => $"{Sheet}!{Number}";
}
