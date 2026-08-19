using ProductEntity = Catalog.Domain.Products.Entities.Product;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.GetAdminList.Specifications;

public sealed class GetAdminProductsSpec : Specification<ProductEntity, ProductListItem>
{
    public GetAdminProductsSpec(
        string? search,
        ProductType? type,
        IReadOnlyCollection<ProductCategoryId>? categoryIds,
        string? sortBy,
        bool descending,
        int? page = null,
        int? pageSize = null)
    {
        Query.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerms = search
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (searchTerms.Length == 1 && int.TryParse(searchTerms[0], out var id))
            {
                Query.Where(x => x.Id == ProductId.Create(id));
            }
            else
            {
                foreach (var searchTerm in searchTerms)
                {
                    var normalizedTerm = searchTerm.ToLowerInvariant();
                    Query.Where(x =>
                        x.Name.ToLower().Contains(normalizedTerm) ||
                        x.RuName.ToLower().Contains(normalizedTerm));
                }
            }
        }

        if (type.HasValue)
            Query.Where(x => x.Type == type.Value);

        if (categoryIds is { Count: > 0 })
            Query.Where(x => x.Categories.Any(category => categoryIds.Contains(category.Id)));

        switch (sortBy?.Trim().ToLowerInvariant())
        {
            case "id":
                if (descending) Query.OrderByDescending(x => x.Id);
                else Query.OrderBy(x => x.Id);
                break;
            case "createdat":
                if (descending) Query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);
                else Query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id);
                break;
            case "updatedat":
                if (descending) Query.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id);
                else Query.OrderBy(x => x.UpdatedAt).ThenBy(x => x.Id);
                break;
            default:
                if (descending) Query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id);
                else Query.OrderBy(x => x.Name).ThenBy(x => x.Id);
                break;
        }

        if (page.HasValue && pageSize.HasValue)
        {
            Query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        Query.Select(x => new ProductListItem(
            x.Id.Value,
            x.Name,
            x.Slug.Value,
            x.Type,
            x.Categories.Select(category => category.CategoryId.Value).ToList(),
            x.Photos.Where(photo => photo.IsMain)
                .Select(photo => (int?)photo.MediaFileId.Value)
                .FirstOrDefault(),
            x.CreatedAt,
            x.UpdatedAt));
    }
}
