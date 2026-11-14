using BuildingBlocks.Domain.Results;

namespace Production.Domain.Orders;

/// <summary>
/// Texts of an order: trimmed; empty becomes null. <see cref="Check"/> reports a missing or too long text as a rule;
/// <see cref="Optional"/> and <see cref="Required"/> then normalize and only guard values already checked.
/// </summary>
public static class OrderText
{
    public static DomainResult Check(string? value, int maxLength, string field, bool required = false)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return required ? ProductionTextErrors.Required(field) : DomainResult.Success();
        return text.Length > maxLength ? ProductionTextErrors.TooLong(field, maxLength) : DomainResult.Success();
    }

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
