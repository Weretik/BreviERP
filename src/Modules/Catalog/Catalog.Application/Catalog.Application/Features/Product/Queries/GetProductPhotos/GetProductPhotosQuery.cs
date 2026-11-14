using Catalog.Application.Features.Product.Shared.Photos;

namespace Catalog.Application.Features.Product.Queries.GetProductPhotos;

/// <summary>The tab «Фото» of a product card (CAT-016): main and other photos with their previews.</summary>
public sealed record GetProductPhotosQuery(int ProductId) : IQuery<Result<ProductPhotoList>>;
