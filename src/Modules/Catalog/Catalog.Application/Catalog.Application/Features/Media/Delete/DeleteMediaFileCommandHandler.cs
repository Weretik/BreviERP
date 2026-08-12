using Catalog.Application.Contracts.Storage;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Media.Shared.Specifications;
using Catalog.Application.Features.Media.Delete.Specifications;
using Catalog.Domain.Media.Entities;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Media.Delete;

public sealed class DeleteMediaFileCommandHandler(
    IMediaStorageService mediaStorageService,
    ICatalogRepository<MediaFile> repository,
    ICatalogReadRepository<ProductEntity> productRepository)
    : ICommandHandler<DeleteMediaFileCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMediaFileCommand command, CancellationToken cancellationToken)
    {
        var mediaFile = await repository.FirstOrDefaultAsync(new MediaFileByIdSpec(command.Id), cancellationToken);
        if (mediaFile is null)
        {
            return Result.NotFound();
        }

        if (await productRepository.AnyAsync(new ProductUsesMediaFileSpec(command.Id), cancellationToken))
            return Result.Conflict("Media file is used by one or more products.");

        await mediaStorageService.DeleteAsync(mediaFile.StorageKey, cancellationToken);
        await repository.DeleteAsync(mediaFile, cancellationToken);

        return Result.Success();
    }
}
