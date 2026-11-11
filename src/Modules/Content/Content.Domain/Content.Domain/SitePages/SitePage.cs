namespace Content.Domain.SitePages;

/// <summary>
/// Search texts of one fixed page of the public site in Ukrainian and Russian (CNT-007): the title of the browser tab
/// and of the search result, and the description under it. An empty text leaves the site's own text.
/// </summary>
public sealed class SitePage
{
    public const int KeyMaxLength = 40;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 320;

    private SitePage() { }

    public SitePage(string key, string updatedBy, DateTimeOffset now)
    {
        Key = key;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public string Key { get; private set; } = null!;

    public string? TitleUk { get; private set; }

    public string? DescriptionUk { get; private set; }

    public string? TitleRu { get; private set; }

    public string? DescriptionRu { get; private set; }

    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public void Replace(string? titleUk, string? descriptionUk, string? titleRu, string? descriptionRu,
        string updatedBy, DateTimeOffset now)
    {
        TitleUk = Clean(titleUk);
        DescriptionUk = Clean(descriptionUk);
        TitleRu = Clean(titleRu);
        DescriptionRu = Clean(descriptionRu);
        if (TitleUk?.Length > TitleMaxLength || TitleRu?.Length > TitleMaxLength)
            throw new ArgumentException("Title is too long.");
        if (DescriptionUk?.Length > DescriptionMaxLength || DescriptionRu?.Length > DescriptionMaxLength)
            throw new ArgumentException("Description is too long.");
        Revision++;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    /// <summary>Trims the text and joins lines: a title or description is one line.</summary>
    public static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
