using Catalog.Application.Features.Product.Photos.DTOs;

namespace Catalog.Application.Features.Product.Photos.GetPhotos;

/// <summary>The tab «Фото» of a product card (CAT-016): main and other photos with their previews.</summary>
public sealed record GetProductPhotosQuery(int ProductId) : IQuery<Result<ProductPhotoList>>;
