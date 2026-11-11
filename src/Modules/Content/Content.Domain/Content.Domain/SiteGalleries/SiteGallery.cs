using System.Text.RegularExpressions;
using Content.Domain.SiteCompany;

namespace Content.Domain.SiteGalleries;

/// <summary>
/// «Контент → Наші роботи» and «Контент → Сертифікати» (CNT-011): the pictures of one page of the site as a whole list,
/// saved with a revision check. The migration seeds the pictures the site kept among its own files.
/// </summary>
public sealed partial class SiteGallery
{
    public const string Works = "works";
    public const string Certificates = "certificates";
    public const int KeyMaxLength = 20;
    public const int MaxWorks = 300;
    public const int MaxCertificates = 40;
    public const int TitleMaxLength = 300;
    public const int NoteMaxLength = 600;
    public const int MaxStandards = 10;
    public const int StandardMaxLength = 200;

    public static readonly IReadOnlyList<string> Keys = [Works, Certificates];

    private SiteGallery() { }

    public SiteGallery(string key, string updatedBy, DateTimeOffset now)
    {
        if (!IsKey(key)) throw new ArgumentException("Unknown gallery.", nameof(key));
        Key = key;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public string Key { get; private set; } = null!;

    public IReadOnlyList<SiteGalleryItem> Items { get; private set; } = [];

    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public static bool IsKey(string? key) => key is Works or Certificates;

    public static int MaxItems(string key) => key == Certificates ? MaxCertificates : MaxWorks;

    /// <summary>Replaces the list as a whole; texts are trimmed and empty ones dropped.</summary>
    public void Replace(IReadOnlyList<SiteGalleryItem> items, string updatedBy, DateTimeOffset now)
    {
        var cleaned = items.Select(Clean).ToArray();
        if (cleaned.Length > MaxItems(Key)) throw new ArgumentException("Too many pictures.");
        if (cleaned.Any(x => x.PhotoId is null && x.Legacy is null)) throw new ArgumentException("A picture is missing.");
        if (Key == Certificates && cleaned.Any(x => string.IsNullOrEmpty(x.Title?.Uk)))
            throw new ArgumentException("A certificate needs a title.");
        Items = cleaned;
        Touch(updatedBy, now);
    }

    /// <summary>Puts a library photo in place of a site file after the transfer, keeping the order and the texts.</summary>
    public void UseLibraryPhoto(string legacyPath, int photoId, string updatedBy, DateTimeOffset now)
    {
        Items = Items.Select(x => x.Legacy?.Path == legacyPath ? x with { PhotoId = photoId, Legacy = null } : x).ToArray();
        Touch(updatedBy, now);
    }

    /// <summary>A file of the site's own pictures: <c>/assets/works/…</c> or <c>/assets/certificates/…</c>, WebP.</summary>
    public static bool IsLegacyPath(string? path) => path is not null && LegacyPath().IsMatch(path);

    private void Touch(string updatedBy, DateTimeOffset now)
    {
        Revision++;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    private static SiteGalleryItem Clean(SiteGalleryItem item)
    {
        var standards = (item.Standards ?? []).Select(SiteCompanyInfo.Clean).Where(x => x is not null).Select(x => x!)
            .ToArray();
        return new SiteGalleryItem(item.PhotoId, item.PhotoId is null ? item.Legacy : null, Text(item.Title),
            Text(item.Note), standards);
    }

    private static SiteText? Text(SiteText? text)
    {
        var uk = SiteCompanyInfo.Clean(text?.Uk);
        var ru = SiteCompanyInfo.Clean(text?.Ru);
        return uk is null && ru is null ? null : new SiteText(uk ?? string.Empty, ru ?? string.Empty);
    }

    [GeneratedRegex("^/assets/(works|certificates)/[a-z0-9-]+\\.webp$")]
    private static partial Regex LegacyPath();
}
