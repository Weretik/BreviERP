using Catalog.Domain.Errors;

namespace Catalog.Domain.Products.Errors;

public static class ProductErrors
{
    public static CatalogDomainError IdIsRequired() =>
        new("Catalog.Product.Id.Required", "Product id must be provided");

    public static CatalogDomainError NameIsRequired() =>
        new("Catalog.Product.Name.Required", "Product name is required");

    public static CatalogDomainError NameLengthInvalid(int maxLength) =>
        new("Catalog.Product.Name.LengthInvalid", $"Product name must be {maxLength} characters or less");

    public static CatalogDomainError SlugIsRequired() =>
        new("Catalog.Product.Slug.Required", "Product slug is required");

    public static CatalogDomainError SlugFormatInvalid() =>
        new("Catalog.Product.Slug.FormatInvalid", "Product slug must contain lowercase letters, numbers, and hyphens only");

    public static CatalogDomainError TypeIsRequired() =>
        new("Catalog.Product.Type.Required", "Product type must be provided");

    public static CatalogDomainError PhotoIdIsRequired() =>
        new("Catalog.Product.Photo.Id.Required", "Product photo id must be provided");

    public static CatalogDomainError PhotoMediaFileIdIsRequired() =>
        new("Catalog.Product.Photo.MediaFileId.Required", "Product photo media file id must be provided");

    public static CatalogDomainError PhotoAltLengthInvalid(int maxLength) =>
        new("Catalog.Product.Photo.Alt.LengthInvalid", $"Product photo alt must be {maxLength} characters or less");

    public static CatalogDomainError PhotoSortOrderInvalid() =>
        new("Catalog.Product.Photo.SortOrder.Invalid", "Product photo sort order must be zero or greater");

    public static CatalogDomainError PhotoMediaFileAlreadyAttached(int mediaFileId) =>
        new("Catalog.Product.Photo.MediaFile.AlreadyAttached", $"Media file '{mediaFileId}' is already attached to the product");

    public static CatalogDomainError PhotoMediaFileNotReady(int mediaFileId) =>
        new("Catalog.Product.Photo.MediaFile.NotReady", $"Media file '{mediaFileId}' is not ready to be attached to the product");

    public static CatalogDomainError PhotoAlreadyExists(int photoId) =>
        new("Catalog.Product.Photo.AlreadyExists", $"Product photo '{photoId}' already exists");

    public static CatalogDomainError PhotoNotFound(int photoId) =>
        new("Catalog.Product.Photo.NotFound", $"Product photo '{photoId}' was not found");
}
