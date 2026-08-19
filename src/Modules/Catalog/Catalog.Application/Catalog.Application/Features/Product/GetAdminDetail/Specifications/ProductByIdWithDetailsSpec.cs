using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminDetail.Specifications;

public sealed class ProductByIdWithDetailsSpec : Specification<ProductEntity>
{
    public ProductByIdWithDetailsSpec(int id)
    {
        Query.AsNoTracking().Where(x => x.Id.Value == id)
            .Include(x => x.Categories).Include(x => x.Photos)
            .Include(x => x.InformationBlocks)
            .Include(x => x.CharacteristicTables).ThenInclude(x => x.Rows)
            .Include(x => x.SewingDetails!).ThenInclude(x => x.Fabrics)
            .Include(x => x.SewingDetails!).ThenInclude(x => x.Accessories)
            .Include(x => x.SewingDetails!).ThenInclude(x => x.Operations)
            .Include(x => x.PpeDetails);
    }
}
