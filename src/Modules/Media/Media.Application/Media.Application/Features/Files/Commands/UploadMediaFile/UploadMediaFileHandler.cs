using Media.Application.Abstractions;
using Media.Domain;
using Media.Domain.Files;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Files.Shared.Images;
using BuildingBlocks.Application;

namespace Media.Application.Features.Files.Commands.UploadMediaFile;

public sealed class UploadMediaFileHandler(
    IMediaDbContext db,
    IMediaStorage storage,
    MediaImagePreparer images,
    MediaUploadSettings settings,
    TimeProvider clock,
    ILogger<UploadMediaFileHandler> logger)
    : ICommandHandler<UploadMediaFileCommand, Result<MediaFileResponse>>
{
    public async ValueTask<Result<MediaFileResponse>> Handle(UploadMediaFileCommand command, CancellationToken cancellationToken)
    {
        if (command.FolderId is { } folderId && !await db.Folders.AnyAsync(x => x.Id == folderId, cancellationToken))
            return Result<MediaFileResponse>.NotFound(FileMessages.FolderNotFound);
        var sourceKey = string.IsNullOrWhiteSpace(command.SourceKey) ? null : command.SourceKey.Trim();
        if (sourceKey is not null && await db.Files.AnyAsync(x => x.SourceKey == sourceKey, cancellationToken))
            return Result<MediaFileResponse>.Conflict(FileMessages.SourceAlreadyImported);

        // The name is checked before the CDN upload, so a rejected name leaves no file behind.
        var name = MediaNames.Normalize(command.DisplayName, MediaFile.FileNameMaxLength, "fileName");
        if (name.IsFailure) return name.Error!.ToResult<MediaFileResponse>();

        var content = await ReadAllAsync(command.Content, cancellationToken);
        var storageKey = $"/{settings.BaseFolder.Trim().Trim('/')}/{Guid.NewGuid():N}{UploadMediaFileCommand.Extensions[command.ContentType]}";
        string publicUrl;
        using (var original = new MemoryStream(content, writable: false))
            publicUrl = await storage.UploadAsync(original, storageKey, command.ContentType, cancellationToken);

        var now = clock.GetUtcNow();
        var file = MediaFile.Create(
            name.Value,
            command.FolderId,
            command.ContentType,
            command.SizeInBytes,
            storage.ProviderName,
            storageKey,
            publicUrl,
            now,
            sourceKey).Value;
        var image = await TryPrepareAsync(content, command.DisplayName, cancellationToken);
        if (image is not null)
            file.SetImage(image.Width, image.Height, image.ThumbnailStorageKey, image.ThumbnailUrl, now);

        db.Files.Add(file);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // The CDN already has the files; remove them so they do not stay without a library row.
            await TryDeleteAsync(storageKey);
            if (image?.ThumbnailStorageKey is { } thumbnailKey) await TryDeleteAsync(thumbnailKey);
            throw;
        }

        return Result.Success(MediaFileResponse.From(file));
    }

    /// <summary>A preview failure never blocks the upload: the photo stays unprocessed and the nightly backfill retries it.</summary>
    private async Task<PreparedMediaImage?> TryPrepareAsync(byte[] content, string name, CancellationToken cancellationToken)
    {
        try
        {
            return await images.PrepareAsync(content, cancellationToken);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger.LogWarning(error, "Preview for «{Name}» was not made; the nightly sync will retry", name);
            return null;
        }
    }

    private static async Task<byte[]> ReadAllAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek) stream.Position = 0;
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }

    private async Task TryDeleteAsync(string key)
    {
        try { await storage.DeleteAsync(key, CancellationToken.None); }
        catch (Exception cleanup) { logger.LogWarning(cleanup, "Could not remove orphan CDN file {StorageKey}", key); }
    }
}
