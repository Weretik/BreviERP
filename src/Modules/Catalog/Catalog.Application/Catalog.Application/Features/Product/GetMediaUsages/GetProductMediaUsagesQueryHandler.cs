using Catalog.Application.Contracts.Persistence;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.GetMediaUsages;

public sealed class GetProductMediaUsagesQueryHandler(ICatalogReadRepository<ProductEntity> repository)
    : IQueryHandler<GetProductMediaUsagesQuery, Result<IReadOnlyList<ProductMediaUsage>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductMediaUsage>>> Handle(GetProductMediaUsagesQuery query, CancellationToken cancellationToken)
    {
        var ids = query.MediaFileIds.Where(x => x > 0).Distinct().ToArray();
        if (ids.Length == 0) return Result.Success<IReadOnlyList<ProductMediaUsage>>([]);
        var products = await repository.ListAsync(new ProductMediaUsagesSpec(ids), cancellationToken);
        IReadOnlyList<ProductMediaUsage> usages = products
            .SelectMany(product => product.Photos
                .Where(photo => ids.Contains(photo.MediaFileId.Value))
                .Select(photo => new ProductMediaUsage(photo.MediaFileId.Value, product.Id.Value, product.Name)))
            .ToList();
        return Result.Success(usages);
    }
}
