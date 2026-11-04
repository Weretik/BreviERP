using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Photos.DTOs;
using Catalog.Application.Features.Product.Photos.Specifications;
using Catalog.Domain.Media.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Photos.SetPhotos;

public sealed class SetProductPhotosCommandHandler(
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

        product.SetPhotoList(command.MediaFileIds.Select(MediaFileId.Create).ToList(),
            command.MainMediaFileId is { } main ? MediaFileId.Create(main) : null, timeProvider.GetUtcNow());
        await repository.UpdateAsync(product, cancellationToken);
        return Result.Success(await ProductPhotoListBuilder.BuildAsync(product, media, cancellationToken));
    }
}
