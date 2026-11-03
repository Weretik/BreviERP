using Media.Application.Abstractions;
using Media.Application.Contracts;
using Media.Domain;
using Media.Domain.Folders;

namespace Media.Application.Folders.CreateFolder;

public sealed class CreateMediaFolderCommandHandler(IMediaDbContext db, TimeProvider clock)
    : ICommandHandler<CreateMediaFolderCommand, Result<MediaFolderResponse>>
{
    public async ValueTask<Result<MediaFolderResponse>> Handle(CreateMediaFolderCommand command, CancellationToken cancellationToken)
    {
        var name = MediaNames.Normalize(command.Input.Name, MediaFolder.NameMaxLength);
        var parentId = command.Input.ParentId;
        var tree = await FolderLookup.LoadTreeAsync(db, cancellationToken);
        if (parentId is { } parent && !tree.Contains(parent))
            return Result<MediaFolderResponse>.NotFound(FolderMessages.ParentNotFound);
        if (!tree.CanAddUnder(parentId))
            return Result<MediaFolderResponse>.Conflict(FolderMessages.TooDeep);
        if (await FolderLookup.NameTakenAsync(db, name, parentId, null, cancellationToken))
            return Result<MediaFolderResponse>.Conflict(FolderMessages.DuplicateName);

        var folder = new MediaFolder(name, parentId, clock.GetUtcNow());
        db.Folders.Add(folder);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(new MediaFolderResponse(folder.Id, folder.Name, folder.ParentId, 0));
    }
}
