namespace Media.Domain;

/// <summary>Display names of folders and photos: trimmed, single spaces, no path separators.</summary>
public static class MediaNames
{
    public static string Normalize(string value, int maxLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalized = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length == 0) throw new ArgumentException("Name is required.", nameof(value));
        if (normalized.Length > maxLength) throw new ArgumentException($"Name must not exceed {maxLength} characters.", nameof(value));
        if (normalized.IndexOfAny(['/', '\\']) >= 0) throw new ArgumentException("Name must not contain / or \\.", nameof(value));
        return normalized;
    }

    public static bool IsValid(string? value, int maxLength)
    {
        if (value is null) return false;
        try
        {
            Normalize(value, maxLength);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
