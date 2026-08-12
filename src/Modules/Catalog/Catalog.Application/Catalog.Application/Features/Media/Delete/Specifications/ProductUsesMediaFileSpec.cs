using Catalog.Domain.Media.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Media.Delete.Specifications;

public sealed class ProductUsesMediaFileSpec : Specification<ProductEntity>
{
    public ProductUsesMediaFileSpec(int mediaFileId)
    {
        var id = MediaFileId.Create(mediaFileId);
        Query.Where(x => x.Photos.Any(photo => photo.MediaFileId == id));
    }
}
