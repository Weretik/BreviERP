using Catalog.Application.Contracts.Media;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Photos.DTOs;
using Catalog.Application.Features.Product.Photos.Specifications;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Photos.GetPhotos;

public sealed class GetProductPhotosQueryHandler(ICatalogReadRepository<ProductEntity> repository, IProductMediaReader media)
    : IQueryHandler<GetProductPhotosQuery, Result<ProductPhotoList>>
{
    public async ValueTask<Result<ProductPhotoList>> Handle(GetProductPhotosQuery query, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductPhotosByIdSpec(query.ProductId, false), cancellationToken);
        return product is null
            ? Result.NotFound()
            : Result.Success(await ProductPhotoListBuilder.BuildAsync(product, media, cancellationToken));
    }
}
