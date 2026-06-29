namespace Catalog.Api.Contracts.Media;

public sealed record MediaFileListItemResponse(
    int Id,
    string OriginalFileName,
    string PublicUrl,
    string ContentType,
    string StorageKey,
    string Status);
