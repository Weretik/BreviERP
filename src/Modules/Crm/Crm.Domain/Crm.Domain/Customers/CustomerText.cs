namespace Crm.Domain.Customers;

public static class CustomerText
{
    public static string Required(string? value, int max, string field)
    {
        var text = Optional(value, max, field);
        return text ?? throw new CustomerValidationException($"{field}: required");
    }

    public static string? Optional(string? value, int max, string field)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        if (text.Length > max) throw new CustomerValidationException($"{field}: too_long");
        return text;
    }

    public static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
