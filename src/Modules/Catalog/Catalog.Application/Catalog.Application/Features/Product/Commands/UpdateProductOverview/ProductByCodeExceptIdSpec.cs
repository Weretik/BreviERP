using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.UpdateProductOverview;

/// <summary>Another product of the same type with this code: model ID for sewing, site article for PPE.</summary>
public sealed class ProductByCodeExceptIdSpec : Specification<ProductEntity>
{
    public ProductByCodeExceptIdSpec(ProductType type, string code, int exceptId)
    {
        var productId = ProductId.Create(exceptId);
        if (type == ProductType.Ppe)
            Query.Where(x => x.Id != productId && x.CatalogProfile != null && x.CatalogProfile.SaleSku == code);
        else
            Query.Where(x => x.Id != productId && x.ModelCode == code);
    }
}
