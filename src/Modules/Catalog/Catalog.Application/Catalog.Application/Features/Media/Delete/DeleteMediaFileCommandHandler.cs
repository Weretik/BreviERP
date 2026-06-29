using Catalog.Application.Contracts.Storage;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Media.Shared.Specifications;
using Catalog.Domain.Media.Entities;

namespace Catalog.Application.Features.Media.Delete;

public sealed class DeleteMediaFileCommandHandler(
    IMediaStorageService mediaStorageService,
    ICatalogRepository<MediaFile> repository)
    : ICommandHandler<DeleteMediaFileCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMediaFileCommand command, CancellationToken cancellationToken)
    {
        var mediaFile = await repository.FirstOrDefaultAsync(new MediaFileByIdSpec(command.Id), cancellationToken);
        if (mediaFile is null)
        {
            return Result.NotFound();
        }

        await mediaStorageService.DeleteAsync(mediaFile.StorageKey, cancellationToken);
        await repository.DeleteAsync(mediaFile, cancellationToken);

        return Result.Success();
    }
}
