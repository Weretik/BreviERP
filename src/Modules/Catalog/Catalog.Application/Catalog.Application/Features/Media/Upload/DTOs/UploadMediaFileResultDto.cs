namespace Catalog.Application.Features.Media.Upload.DTOs;

public sealed record UploadMediaFileResultDto(
    int MediaFileId,
    string StorageKey,
    string PublicUrl,
    string ContentType);
