using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Domain.Products.Enums;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.GetList.Specifications;

public sealed class ProductOverviewSpec : Specification<ProductEntity, ProductOverviewReadModel>
{
    public ProductOverviewSpec(ProductType? type)
    {
        Query.AsNoTracking();
        if (type is { } productType) Query.Where(x => x.Type == productType);
        Query.OrderBy(x => x.Name);
        Query.Select(x => new ProductOverviewReadModel(
            x.Id.Value, x.Type, x.ModelCode, x.CatalogProfile != null ? x.CatalogProfile.SaleSku : null, x.Name,
            x.CatalogProfile != null ? x.CatalogProfile.SectionId : null,
            x.CatalogProfile != null ? x.CatalogProfile.Season : null,
            x.CatalogProfile == null || x.CatalogProfile.IsActive,
            x.CatalogProfile != null && x.CatalogProfile.IsSourceReviewed,
            x.CatalogProfile != null ? x.CatalogProfile.SourceReviewDate : null,
            x.CatalogProfile != null ? x.CatalogProfile.WebsiteUrl : null,
            x.CatalogProfile != null ? x.CatalogProfile.ShortDescription : null,
            x.CreatedAt, x.UpdatedAt));
    }
}
