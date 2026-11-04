namespace Crm.Application.Quotes.Sending;

/// <summary>Recipient addresses typed in one field, separated by commas, semicolons or spaces.</summary>
public static class QuoteRecipients
{
    private static readonly char[] Separators = [',', ';', ' ', '\n', '\r', '\t'];

    public static string[] Parse(string? text) =>
        (text ?? "").Split(Separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
