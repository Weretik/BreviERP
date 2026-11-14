using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;

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
            // A slug that breaks the format cannot be stored, so no product has it.
            var value = ProductSlug.Create(slug);
            if (value.IsFailure)
                Query.Where(x => false);
            else
            {
                var stored = value.Value;
                Query.Where(x => x.Slug == stored);
            }
        }
        Query.AsNoTracking();
    }
}
