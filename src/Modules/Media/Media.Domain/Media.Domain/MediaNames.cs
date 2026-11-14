using BuildingBlocks.Domain.Results;

namespace Media.Domain;

/// <summary>Display names of folders and photos: trimmed, single spaces, no path separators.</summary>
public static class MediaNames
{
    public static DomainResult<string> Normalize(string? value, int maxLength, string field = "name")
    {
        var normalized = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length == 0) return MediaErrors.NameRequired(field);
        if (normalized.Length > maxLength) return MediaErrors.NameTooLong(field, maxLength);
        if (normalized.IndexOfAny(['/', '\\']) >= 0) return MediaErrors.NameHasSlash(field);
        return normalized;
    }

    public static bool IsValid(string? value, int maxLength) => value is not null && Normalize(value, maxLength).IsSuccess;
}
