namespace Catalog.Application.Contracts.Storage;

public interface IMediaStorageService
{
    Task<StoredMediaFileResult> UploadAsync(
        Stream fileStream,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken);
}
