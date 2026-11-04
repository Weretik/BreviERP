namespace Production.Domain;

/// <summary>Trims and collapses spaces in names typed by staff; the normalized key ignores case.</summary>
public static class ProductionText
{
    public static string Clean(string? value, int maxLength)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (text.Length == 0) throw new ArgumentException("Value is required.", nameof(value));
        if (text.Length > maxLength) throw new ArgumentException($"Value is longer than {maxLength}.", nameof(value));
        return text;
    }

    public static string Key(string value) => Clean(value, int.MaxValue).ToLowerInvariant();
}
