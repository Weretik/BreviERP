using Catalog.Domain.Media;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Features.Product.Shared.Photos;
using BuildingBlocks.Application;

namespace Catalog.Application.Features.Product.Commands.SetProductPhotos;

public sealed class SetProductPhotosHandler(
    ICatalogRepository<ProductEntity> repository,
    IProductMediaReader media,
    TimeProvider timeProvider) : ICommandHandler<SetProductPhotosCommand, Result<ProductPhotoList>>
{
    public async ValueTask<Result<ProductPhotoList>> Handle(SetProductPhotosCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductPhotosByIdSpec(command.ProductId, true), cancellationToken);
        if (product is null) return Result.NotFound();

        var attached = product.Photos.Select(x => x.MediaFileId.Value).ToHashSet();
        var added = command.MediaFileIds.Where(x => !attached.Contains(x)).ToList();
        if (added.Count > 0)
        {
            var found = await media.GetFilesAsync(added, cancellationToken);
            var missing = added.FirstOrDefault(x => !found.ContainsKey(x));
            if (missing != 0) return Result.Conflict($"Фото {missing} не знайдено в медіатеці.");
        }

        var set = product.SetPhotoList(command.MediaFileIds.Select(MediaFileId.Create).ToList(),
            command.MainMediaFileId is { } main ? MediaFileId.Create(main) : null, timeProvider.GetUtcNow());
        if (set.IsFailure) return set.Error!.ToResult<ProductPhotoList>();
        await repository.UpdateAsync(product, cancellationToken);
        return Result.Success(await ProductPhotoListBuilder.BuildAsync(product, media, cancellationToken));
    }
}
