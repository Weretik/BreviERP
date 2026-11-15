using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.ReplaceProduct;

/// <summary>The product with every collection the save replaces, one query per collection (database rules 1.3).</summary>
public sealed class ProductByIdSpec : Specification<ProductEntity>
{
    public ProductByIdSpec(int id) => Query.AsSplitQuery().Where(x => x.Id == ProductId.Create(id))
        .Include(x => x.Articles)
        .Include(x => x.MaterialLines)
        .Include(x => x.OperationLines)
        .Include(x => x.SewingDetails!).ThenInclude(x => x.Fabrics)
        .Include(x => x.SewingDetails!).ThenInclude(x => x.Accessories)
        .Include(x => x.SewingDetails!).ThenInclude(x => x.Operations);
}
