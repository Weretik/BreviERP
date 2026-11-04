using Media.Application.Abstractions;

namespace Media.Application.Files.DeleteFile;

public sealed class DeleteMediaFileCommandHandler(IMediaDbContext db, IMediaStorage storage, IMediaUsageReader usages)
    : ICommandHandler<DeleteMediaFileCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMediaFileCommand command, CancellationToken cancellationToken)
    {
        var file = await db.Files.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (file is null) return Result.NotFound(FileMessages.NotFound);
        var used = await usages.FindAsync([file.Id], cancellationToken);
        if (used.Count > 0)
            return Result.Conflict($"{FileMessages.InUse}: {string.Join(", ", used.Select(x => $"{x.Area} «{x.Title}»"))}.");

        await storage.DeleteAsync(file.StorageKey, cancellationToken);
        if (file.ThumbnailStorageKey is { } thumbnailKey) await storage.DeleteAsync(thumbnailKey, cancellationToken);
        db.Files.Remove(file);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
