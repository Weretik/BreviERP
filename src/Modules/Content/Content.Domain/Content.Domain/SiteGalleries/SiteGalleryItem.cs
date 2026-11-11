using Content.Domain.SiteCompany;

namespace Content.Domain.SiteGalleries;

/// <summary>
/// A picture the site kept among its own files before it moved to the «Фото» library (CNT-011): the large file, its
/// small copy and the pixel size of the small copy.
/// </summary>
public sealed record SiteGalleryLegacyPhoto(string Path, string ThumbPath, int Width, int Height);

/// <summary>
/// One picture of «Наші роботи» or one document of «Сертифікати», in the order staff set. The picture is a photo of the
/// library (<see cref="PhotoId"/>) or, until the transfer, a file of the site (<see cref="Legacy"/>). Works use
/// <see cref="Title"/> as the picture description; certificates show the title, the note on what clothing the document
/// covers, and the standards.
/// </summary>
public sealed record SiteGalleryItem(
    int? PhotoId,
    SiteGalleryLegacyPhoto? Legacy,
    SiteText? Title,
    SiteText? Note,
    IReadOnlyList<string>? Standards);
