using Media.Application.Abstractions;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.DeleteMediaFolder;

public sealed class DeleteMediaFolderHandler(IMediaDbContext db) : ICommandHandler<DeleteMediaFolderCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMediaFolderCommand command, CancellationToken cancellationToken)
    {
        var folder = await db.Folders.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (folder is null) return Result.NotFound(FolderMessages.NotFound);
        var hasContent = await db.Folders.AnyAsync(x => x.ParentId == folder.Id, cancellationToken)
                         || await db.Files.AnyAsync(x => x.FolderId == folder.Id, cancellationToken);
        if (hasContent) return Result.Conflict(FolderMessages.NotEmpty);

        db.Folders.Remove(folder);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
