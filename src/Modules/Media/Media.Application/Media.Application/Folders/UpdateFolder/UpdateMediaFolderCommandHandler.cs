using Media.Application.Abstractions;
using Media.Application.Contracts;
using Media.Domain;
using Media.Domain.Folders;

namespace Media.Application.Folders.UpdateFolder;

public sealed class UpdateMediaFolderCommandHandler(IMediaDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateMediaFolderCommand, Result<MediaFolderResponse>>
{
    public async ValueTask<Result<MediaFolderResponse>> Handle(UpdateMediaFolderCommand command, CancellationToken cancellationToken)
    {
        var folder = await db.Folders.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (folder is null) return Result<MediaFolderResponse>.NotFound(FolderMessages.NotFound);

        var name = MediaNames.Normalize(command.Input.Name, MediaFolder.NameMaxLength);
        var parentId = command.Input.ParentId;
        var now = clock.GetUtcNow();
        if (parentId != folder.ParentId)
        {
            var tree = await FolderLookup.LoadTreeAsync(db, cancellationToken);
            if (parentId is { } parent && !tree.Contains(parent))
                return Result<MediaFolderResponse>.NotFound(FolderMessages.ParentNotFound);
            if (parentId is { } target && tree.SelfAndDescendants(folder.Id).Contains(target))
                return Result<MediaFolderResponse>.Conflict(FolderMessages.MoveIntoItself);
            if (!tree.CanMove(folder.Id, parentId))
                return Result<MediaFolderResponse>.Conflict(FolderMessages.TooDeep);
        }

        if (await FolderLookup.NameTakenAsync(db, name, parentId, folder.Id, cancellationToken))
            return Result<MediaFolderResponse>.Conflict(FolderMessages.DuplicateName);

        folder.Rename(name, now);
        if (parentId != folder.ParentId) folder.MoveTo(parentId, now);
        await db.SaveChangesAsync(cancellationToken);
        var count = await db.Files.CountAsync(x => x.FolderId == folder.Id, cancellationToken);
        return Result.Success(new MediaFolderResponse(folder.Id, folder.Name, folder.ParentId, count));
    }
}
