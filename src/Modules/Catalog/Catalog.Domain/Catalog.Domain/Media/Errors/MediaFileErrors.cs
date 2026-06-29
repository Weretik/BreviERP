using Catalog.Domain.Errors;

namespace Catalog.Domain.Media.Errors;

public static class MediaFileErrors
{
    public static CatalogDomainError IdIsRequired() =>
        new("Catalog.MediaFile.Id.Required", "Media file id must be provided");

    public static CatalogDomainError FileNameIsRequired() =>
        new("Catalog.MediaFile.FileName.Required", "Media file name is required");

    public static CatalogDomainError FileNameLengthInvalid(int maxLength) =>
        new("Catalog.MediaFile.FileName.LengthInvalid", $"Media file name must be {maxLength} characters or less");

    public static CatalogDomainError ContentTypeIsRequired() =>
        new("Catalog.MediaFile.ContentType.Required", "Media file content type is required");

    public static CatalogDomainError ContentTypeLengthInvalid(int maxLength) =>
        new("Catalog.MediaFile.ContentType.LengthInvalid", $"Media file content type must be {maxLength} characters or less");

    public static CatalogDomainError SizeInvalid() =>
        new("Catalog.MediaFile.Size.Invalid", "Media file size must be greater than zero");

    public static CatalogDomainError StorageProviderIsRequired() =>
        new("Catalog.MediaFile.StorageProvider.Required", "Storage provider is required");

    public static CatalogDomainError StorageProviderLengthInvalid(int maxLength) =>
        new("Catalog.MediaFile.StorageProvider.LengthInvalid", $"Storage provider must be {maxLength} characters or less");

    public static CatalogDomainError BucketNameIsRequired() =>
        new("Catalog.MediaFile.BucketName.Required", "Storage bucket name is required");

    public static CatalogDomainError BucketNameLengthInvalid(int maxLength) =>
        new("Catalog.MediaFile.BucketName.LengthInvalid", $"Storage bucket name must be {maxLength} characters or less");

    public static CatalogDomainError StorageKeyIsRequired() =>
        new("Catalog.MediaFile.StorageKey.Required", "Storage key is required");

    public static CatalogDomainError StorageKeyLengthInvalid(int maxLength) =>
        new("Catalog.MediaFile.StorageKey.LengthInvalid", $"Storage key must be {maxLength} characters or less");

    public static CatalogDomainError PublicUrlIsRequired() =>
        new("Catalog.MediaFile.PublicUrl.Required", "Public url is required");

    public static CatalogDomainError PublicUrlLengthInvalid(int maxLength) =>
        new("Catalog.MediaFile.PublicUrl.LengthInvalid", $"Public url must be {maxLength} characters or less");

    public static CatalogDomainError WidthInvalid() =>
        new("Catalog.MediaFile.Width.Invalid", "Width must be greater than zero");

    public static CatalogDomainError HeightInvalid() =>
        new("Catalog.MediaFile.Height.Invalid", "Height must be greater than zero");

    public static CatalogDomainError UploadCompletionInvalidState() =>
        new("Catalog.MediaFile.Status.InvalidForUploadCompletion", "Only pending media files can be marked as uploaded");
}
