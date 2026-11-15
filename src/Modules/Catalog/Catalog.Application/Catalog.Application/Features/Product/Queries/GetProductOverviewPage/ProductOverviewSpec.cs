using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewPage;

/// <summary>The «Товари» fields of every product of a type, or of one product when <paramref name="id"/> is set.</summary>
public sealed class ProductOverviewSpec : Specification<ProductEntity, ProductOverviewReadModel>
{
    public ProductOverviewSpec(ProductType? type, IReadOnlyCollection<int>? ids = null)
    {
        Query.AsNoTracking().Where(x => !x.IsDeleted);
        if (type is { } productType) Query.Where(x => x.Type == productType);
        if (ids is not null)
        {
            var keys = ids.Distinct().Select(ProductId.Create).ToList();
            Query.Where(x => keys.Contains(x.Id));
        }
        Query.Select(x => new ProductOverviewReadModel(
            x.Id.Value, x.Type, x.ModelCode, x.CatalogProfile != null ? x.CatalogProfile.SaleSku : null, x.Name,
            x.CatalogProfile != null ? x.CatalogProfile.SectionId : null,
            x.CatalogProfile != null ? x.CatalogProfile.Season : null,
            x.CatalogProfile == null || x.CatalogProfile.IsActive,
            x.CatalogProfile != null ? x.CatalogProfile.WebsiteUrl : null,
            x.CatalogProfile != null ? x.CatalogProfile.ShortDescription : null,
            x.CreatedAt, x.UpdatedAt,
            x.Photos.Where(photo => photo.IsVisible).OrderByDescending(photo => photo.IsMain)
                .ThenBy(photo => photo.SortOrder).Select(photo => (int?)photo.MediaFileId.Value).FirstOrDefault()));
    }
}
