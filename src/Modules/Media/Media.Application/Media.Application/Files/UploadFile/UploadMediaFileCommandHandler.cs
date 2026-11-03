using Media.Application.Abstractions;
using Media.Application.Contracts;
using Media.Domain.Files;

namespace Media.Application.Files.UploadFile;

public sealed class UploadMediaFileCommandHandler(
    IMediaDbContext db,
    IMediaStorage storage,
    MediaUploadSettings settings,
    TimeProvider clock,
    ILogger<UploadMediaFileCommandHandler> logger)
    : ICommandHandler<UploadMediaFileCommand, Result<MediaFileResponse>>
{
    public async ValueTask<Result<MediaFileResponse>> Handle(UploadMediaFileCommand command, CancellationToken cancellationToken)
    {
        if (command.FolderId is { } folderId && !await db.Folders.AnyAsync(x => x.Id == folderId, cancellationToken))
            return Result<MediaFileResponse>.NotFound(FileMessages.FolderNotFound);

        var storageKey = $"/{settings.BaseFolder.Trim().Trim('/')}/{Guid.NewGuid():N}{UploadMediaFileCommand.Extensions[command.ContentType]}";
        var publicUrl = await storage.UploadAsync(command.Content, storageKey, command.ContentType, cancellationToken);

        var file = new MediaFile(
            command.DisplayName,
            command.FolderId,
            command.ContentType,
            command.SizeInBytes,
            storage.ProviderName,
            storageKey,
            publicUrl,
            clock.GetUtcNow());
        db.Files.Add(file);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // The CDN already has the file; remove it so it does not stay without a library row.
            try { await storage.DeleteAsync(storageKey, CancellationToken.None); }
            catch (Exception cleanup) { logger.LogWarning(cleanup, "Could not remove orphan CDN file {StorageKey}", storageKey); }
            throw;
        }

        return Result.Success(MediaFileResponse.From(file));
    }
}
