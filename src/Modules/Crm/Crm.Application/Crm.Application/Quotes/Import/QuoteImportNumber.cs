using System.Text.RegularExpressions;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Import;

/// <summary>The calculator number `n-month-year`, the same form as a Brevi one (R-03).</summary>
public static partial class QuoteImportNumber
{
    public static bool IsValid(string? text) => text is not null && Pattern().IsMatch(text.Trim()) &&
        Parse(text) is { Month: >= 1 and <= 12, Sequence: > 0, Year: >= 2000 and <= 2100 };

    public static QuoteNumber Parse(string text)
    {
        var match = Pattern().Match(text.Trim());
        return new QuoteNumber(int.Parse(match.Groups[3].Value), int.Parse(match.Groups[2].Value),
            int.Parse(match.Groups[1].Value));
    }

    [GeneratedRegex(@"^(\d{1,6})-(\d{1,2})-(\d{4})$")]
    private static partial Regex Pattern();
}
