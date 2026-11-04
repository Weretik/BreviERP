using Catalog.Application.Features.Product.Photos.DTOs;

namespace Catalog.Application.Features.Product.Photos.SetPhotos;

/// <summary>
/// Saves the tab «Фото» (CAT-016): the product gets exactly these library photos in this order. A null
/// <paramref name="MainMediaFileId"/> keeps the current main photo, or makes the first one main.
/// </summary>
public sealed record SetProductPhotosCommand(int ProductId, IReadOnlyList<int> MediaFileIds, int? MainMediaFileId)
    : ICommand<Result<ProductPhotoList>>
{
    public const int MaxPhotos = 60;
}
