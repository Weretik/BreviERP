using System.Globalization;
using System.Text.RegularExpressions;

namespace Purchasing.Domain.Ingestion;

/// <summary>Recognises the sheet's DateTime cell (DD.MM.YYYY, DD/MM/YYYY or ISO, optional time) for sorting only.</summary>
public static partial class MessageDates
{
    [GeneratedRegex(@"^(\d{1,2})[./](\d{1,2})[./](\d{4})(?:[ T,]+(\d{1,2}):(\d{2})(?::(\d{2}))?)?")]
    private static partial Regex DayFirst();

    [GeneratedRegex(@"^(\d{4})-(\d{2})-(\d{2})(?:[ T](\d{2}):(\d{2})(?::(\d{2}))?)?")]
    private static partial Regex Iso();

    public static DateTime? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        var match = DayFirst().Match(text);
        if (match.Success) return Build(match.Groups[3], match.Groups[2], match.Groups[1], match.Groups[4], match.Groups[5], match.Groups[6]);
        match = Iso().Match(text);
        return match.Success
            ? Build(match.Groups[1], match.Groups[2], match.Groups[3], match.Groups[4], match.Groups[5], match.Groups[6])
            : null;
    }

    private static DateTime? Build(Group year, Group month, Group day, Group hour, Group minute, Group second)
    {
        static int Number(Group group) => group.Success ? int.Parse(group.Value, CultureInfo.InvariantCulture) : 0;
        try
        {
            return new DateTime(Number(year), Number(month), Number(day), Number(hour), Number(minute), Number(second),
                DateTimeKind.Unspecified);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
