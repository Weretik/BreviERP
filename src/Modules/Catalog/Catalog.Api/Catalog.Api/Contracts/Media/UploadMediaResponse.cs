namespace Catalog.Api.Contracts.Media;

public sealed record UploadMediaResponse(
    int MediaFileId,
    string StorageKey,
    string PublicUrl,
    string ContentType,
    string OriginalFileName);
