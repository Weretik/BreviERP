namespace Crm.Domain.Quotes;

internal static class QuoteText
{
    public static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
