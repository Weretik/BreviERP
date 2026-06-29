namespace Catalog.Application.Contracts.Storage;

public sealed record StoredMediaFileResult(
    string StorageKey,
    string PublicUrl,
    string ContentType,
    string StorageProvider,
    string BucketName);
