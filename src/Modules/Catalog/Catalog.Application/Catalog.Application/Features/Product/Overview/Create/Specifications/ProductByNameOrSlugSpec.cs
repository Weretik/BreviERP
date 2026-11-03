using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Overview.Create.Specifications;

/// <summary>A product that already uses this name (in either name column) or slug.</summary>
public sealed class ProductByNameOrSlugSpec : Specification<ProductEntity>
{
    public ProductByNameOrSlugSpec(string name, string? slug = null)
    {
        if (slug is null)
        {
            Query.Where(x => x.Name == name || x.RuName == name);
        }
        else
        {
            var value = ProductSlug.Create(slug);
            Query.Where(x => x.Slug == value);
        }
        Query.AsNoTracking();
    }
}
