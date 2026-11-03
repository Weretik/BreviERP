using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.GetList.Specifications;

/// <summary>The «Товари» fields of every product of a type, or of one product when <paramref name="id"/> is set.</summary>
public sealed class ProductOverviewSpec : Specification<ProductEntity, ProductOverviewReadModel>
{
    public ProductOverviewSpec(ProductType? type, int? id = null)
    {
        Query.AsNoTracking().Where(x => !x.IsDeleted);
        if (type is { } productType) Query.Where(x => x.Type == productType);
        if (id is { } productId)
        {
            var key = ProductId.Create(productId);
            Query.Where(x => x.Id == key);
        }
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
