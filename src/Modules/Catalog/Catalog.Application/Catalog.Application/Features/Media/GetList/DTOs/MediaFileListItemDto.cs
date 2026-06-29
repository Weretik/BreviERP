namespace Catalog.Application.Features.Media.GetList.DTOs;

public sealed record MediaFileListItemDto(
    int Id,
    string OriginalFileName,
    string PublicUrl,
    string ContentType,
    string StorageKey,
    string Status);
