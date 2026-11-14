using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Results;

namespace Production.Domain;

/// <summary>
/// Trims and collapses spaces in names typed by staff; the normalized key ignores case. <see cref="Check"/> reports a
/// missing or too long text as a rule; <see cref="Clean"/> then only guards a value already checked.
/// </summary>
public static class ProductionText
{
    public static DomainResult Check(string? value, int maxLength, string field, bool required = true)
    {
        var text = Collapse(value);
        if (text.Length == 0) return required ? ProductionTextErrors.Required(field) : DomainResult.Success();
        return text.Length > maxLength ? ProductionTextErrors.TooLong(field, maxLength) : DomainResult.Success();
    }

    public static string Clean(string? value, int maxLength)
    {
        var text = Collapse(value);
        if (text.Length == 0) throw new ArgumentException("Value is required.", nameof(value));
        if (text.Length > maxLength) throw new ArgumentException($"Value is longer than {maxLength}.", nameof(value));
        return text;
    }

    /// <summary>The case-free key of a text; empty for an empty text.</summary>
    public static string Key(string? value) => Collapse(value).ToLowerInvariant();

    private static string Collapse(string? value) =>
        string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

/// <summary>Errors of texts typed by staff anywhere in the workshop.</summary>
public static class ProductionTextErrors
{
    public static Error Required(string field) =>
        Error.Validation("production.required", "Заповніть це поле.", field);

    public static Error TooLong(string field, int max) =>
        Error.Validation("production.too_long", $"Не довше {max} символів.", field);
}
