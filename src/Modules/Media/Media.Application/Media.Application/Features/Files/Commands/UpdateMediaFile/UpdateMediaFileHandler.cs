using Media.Application.Abstractions;
using Media.Application.Features.Files.Shared;
using BuildingBlocks.Application;

namespace Media.Application.Features.Files.Commands.UpdateMediaFile;

public sealed class UpdateMediaFileHandler(IMediaDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateMediaFileCommand, Result<MediaFileResponse>>
{
    public async ValueTask<Result<MediaFileResponse>> Handle(UpdateMediaFileCommand command, CancellationToken cancellationToken)
    {
        var file = await db.Files.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (file is null) return Result<MediaFileResponse>.NotFound(FileMessages.NotFound);
        var folderId = command.Input.FolderId;
        if (folderId is { } id && !await db.Folders.AnyAsync(x => x.Id == id, cancellationToken))
            return Result<MediaFileResponse>.NotFound(FileMessages.FolderNotFound);

        var updated = file.Update(command.Input.FileName, command.Input.Alt, folderId, clock.GetUtcNow());
        if (updated.IsFailure) return updated.Error!.ToResult<MediaFileResponse>();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(MediaFileResponse.From(file));
    }
}
