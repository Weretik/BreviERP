using BuildingBlocks.Domain.Results;

namespace Crm.Domain.Customers;

public static class CustomerText
{
    public static DomainResult<string> Required(string? value, int max, string field)
    {
        var text = Optional(value, max, field);
        if (text.IsFailure) return text.Error!;
        return text.Value is { } filled ? filled : CustomerErrors.Required(field);
    }

    public static DomainResult<string?> Optional(string? value, int max, string field)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text)) return DomainResult<string?>.Success(null);
        if (text.Length > max) return CustomerErrors.TooLong(field, max);
        return text;
    }

    public static string Normalize(string value) => value.Trim().ToUpperInvariant();
}
