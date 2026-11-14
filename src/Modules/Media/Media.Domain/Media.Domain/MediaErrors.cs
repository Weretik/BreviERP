
using BuildingBlocks.Domain.Results;

namespace Media.Domain;

/// <summary>Business rule errors of the photo library.</summary>
public static class MediaErrors
{
    public static Error NameRequired(string field) =>
        Error.Validation("media.name_required", "Вкажіть назву.", field);

    public static Error NameTooLong(string field, int maxLength) =>
        Error.Validation("media.name_too_long", $"Назва має бути не довша за {maxLength} символів.", field);

    public static Error NameHasSlash(string field) =>
        Error.Validation("media.name_has_slash", "Назва не може містити / або \\.", field);

    public static Error AltTooLong(int maxLength) =>
        Error.Validation("media.alt_too_long", $"Опис має бути не довший за {maxLength} символів.", "alt");

    public static Error FolderOwnParent() =>
        Error.Conflict("media.folder_own_parent", "Папку не можна перемістити саму в себе.");
}
