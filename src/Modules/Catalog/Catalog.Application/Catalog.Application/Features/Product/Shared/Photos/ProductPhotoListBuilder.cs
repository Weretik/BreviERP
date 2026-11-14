using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions.Media;

namespace Catalog.Application.Features.Product.Shared.Photos;

/// <summary>Turns product photos into the «Фото» tab list with library URLs.</summary>
internal static class ProductPhotoListBuilder
{
    public static async Task<ProductPhotoList> BuildAsync(
        ProductEntity product, IProductMediaReader media, CancellationToken cancellationToken)
    {
        var photos = product.Photos.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToList();
        var files = await media.GetFilesAsync(photos.Select(x => x.MediaFileId.Value).ToList(), cancellationToken);
        return new ProductPhotoList(product.Id.Value, photos.Select(photo =>
        {
            var file = files.GetValueOrDefault(photo.MediaFileId.Value);
            return new ProductPhotoItem(photo.MediaFileId.Value, file?.FileName, file?.PublicUrl, file?.ThumbnailUrl,
                photo.IsMain, photo.SortOrder, file is null);
        }).ToList());
    }
}
