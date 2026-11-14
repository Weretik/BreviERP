using Catalog.Domain.Media;
using ProductEntity = Catalog.Domain.Products.Product;

namespace Catalog.Application.Features.Product.Queries.GetProductMediaUsages;

public sealed class ProductMediaUsagesSpec : Specification<ProductEntity>
{
    public ProductMediaUsagesSpec(IReadOnlyCollection<int> mediaFileIds)
    {
        var ids = mediaFileIds.Select(MediaFileId.Create).ToArray();
        Query.AsNoTracking()
            .Include(x => x.Photos)
            .Where(x => x.Photos.Any(photo => ids.Contains(photo.MediaFileId)))
            .OrderBy(x => x.Name);
    }
}
