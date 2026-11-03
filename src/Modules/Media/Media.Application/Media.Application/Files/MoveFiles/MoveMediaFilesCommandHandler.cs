using Media.Application.Abstractions;

namespace Media.Application.Files.MoveFiles;

public sealed class MoveMediaFilesCommandHandler(IMediaDbContext db, TimeProvider clock) : ICommandHandler<MoveMediaFilesCommand, Result>
{
    public async ValueTask<Result> Handle(MoveMediaFilesCommand command, CancellationToken cancellationToken)
    {
        var folderId = command.Input.FolderId;
        if (folderId is { } id && !await db.Folders.AnyAsync(x => x.Id == id, cancellationToken))
            return Result.NotFound(FileMessages.FolderNotFound);
        var ids = command.Input.FileIds.Distinct().ToList();
        var files = await db.Files.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        if (files.Count != ids.Count) return Result.NotFound(FileMessages.SomeNotFound);

        var now = clock.GetUtcNow();
        foreach (var file in files) file.MoveTo(folderId, now);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
