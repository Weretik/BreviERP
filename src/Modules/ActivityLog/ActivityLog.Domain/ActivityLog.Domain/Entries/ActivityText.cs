namespace ActivityLog.Domain.Entries;

/// <summary>Cuts texts of an entry to their column length, ending with «…» (ACT-001 R-02).</summary>
public static class ActivityText
{
    public static string Cut(string? text, int maxLength)
    {
        var value = (text ?? string.Empty).Trim();
        return value.Length <= maxLength ? value : string.Concat(value.AsSpan(0, maxLength - 1), "…");
    }

    public static string? CutOrNull(string? text, int maxLength) =>
        string.IsNullOrWhiteSpace(text) ? null : Cut(text, maxLength);
}
