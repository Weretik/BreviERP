namespace Production.Domain.Orders;

/// <summary>Texts of an order: trimmed; empty becomes null; too long is a programming error (validators stop it).</summary>
public static class OrderText
{
    public static string? Optional(string? value, int maxLength)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        if (text.Length > maxLength) throw new ArgumentException($"Value is longer than {maxLength}.", nameof(value));
        return text;
    }

    public static string Required(string? value, int maxLength) =>
        Optional(value, maxLength) ?? throw new ArgumentException("Value is required.", nameof(value));
}
