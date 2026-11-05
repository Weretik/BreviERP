namespace Accounting.Domain.Ledger;

/// <summary>Trimming rules shared by finance records.</summary>
public static class FinanceText
{
    /// <summary>Trimmed text, or an empty string for null and blanks; line breaks inside stay.</summary>
    public static string Clean(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    /// <summary>A one-line name: trimmed, with each run of spaces, tabs or line breaks made one space.</summary>
    public static string Name(string? value) =>
        string.Join(' ', Clean(value).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
