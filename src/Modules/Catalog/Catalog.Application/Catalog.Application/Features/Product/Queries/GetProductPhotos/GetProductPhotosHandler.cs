using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Features.Product.Shared.Photos;

namespace Catalog.Application.Features.Product.Queries.GetProductPhotos;

public sealed class GetProductPhotosHandler(ICatalogReadRepository<ProductEntity> repository, IProductMediaReader media)
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
