using BuildingBlocks.Domain.Errors;

namespace Content.Domain.SiteGalleries;

/// <summary>Business rule errors of the site galleries.</summary>
public static class SiteGalleryErrors
{
    public static Error UnknownGallery() =>
        Error.NotFound("content.gallery_not_found", "Такої галереї немає.");

    public static Error TooManyPictures(int max) =>
        Error.Validation("content.gallery_too_many", $"У галереї може бути не більше {max} фото.", "Items");

    public static Error PictureMissing() =>
        Error.Validation("content.gallery_picture_missing", "Виберіть фото для кожного пункту.", "Items");

    public static Error CertificateTitleRequired() =>
        Error.Validation("content.gallery_certificate_title", "Вкажіть назву кожного сертифіката.", "Items");
}
