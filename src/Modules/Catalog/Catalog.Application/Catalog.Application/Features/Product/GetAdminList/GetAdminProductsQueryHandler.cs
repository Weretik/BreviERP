using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.GetAdminList.DTOs;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminList;

public sealed class GetAdminProductsQueryHandler(
    ICatalogReadRepository<ProductEntity> repository,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository) : IQueryHandler<GetAdminProductsQuery, Result<ProductListPage>>
{
    public async ValueTask<Result<ProductListPage>> Handle(GetAdminProductsQuery query, CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(new GetAdminProductsSpec(), cancellationToken);
        IEnumerable<ProductEntity> filtered = items;
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filtered = int.TryParse(term, out var id) ? filtered.Where(x => x.Id.Value == id) : filtered.Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || x.RuName.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Slug.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        if (query.Type.HasValue) filtered = filtered.Where(x => x.Type == query.Type.Value);
        if (query.CategoryId.HasValue)
        {
            var categories = await categoryRepository.ListAsync(cancellationToken);
            var root = categories.FirstOrDefault(x => x.Id.Value == query.CategoryId.Value);
            if (root is null) return Result.Success(new ProductListPage([], Math.Max(1, query.Page), query.PageSize, 0));
            var ids = categories.Where(x => x.Path.StartsWith(root.Path, StringComparison.Ordinal)).Select(x => x.Id.Value).ToHashSet();
            filtered = filtered.Where(x => x.Categories.Any(c => ids.Contains(c.CategoryId.Value)));
        }
        filtered = (query.SortBy.ToLowerInvariant(), query.Descending) switch
        {
            ("id", false) => filtered.OrderBy(x => x.Id.Value), ("id", true) => filtered.OrderByDescending(x => x.Id.Value),
            ("createdat", false) => filtered.OrderBy(x => x.CreatedAt), ("createdat", true) => filtered.OrderByDescending(x => x.CreatedAt),
            ("updatedat", false) => filtered.OrderBy(x => x.UpdatedAt), ("updatedat", true) => filtered.OrderByDescending(x => x.UpdatedAt),
            (_, true) => filtered.OrderByDescending(x => x.Name), _ => filtered.OrderBy(x => x.Name)
        };
        var total = filtered.Count(); var page = Math.Max(1, query.Page); var pageSize = query.PageSize is 10 or 20 or 50 ? query.PageSize : 20;
        var rows = filtered.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new ProductListItem(x.Id.Value, x.Name, x.Slug.Value, x.Type, x.Categories.Select(c => c.CategoryId.Value).ToList(), x.Photos.FirstOrDefault(p => p.IsMain)?.MediaFileId.Value, x.CreatedAt, x.UpdatedAt)).ToList();
        return Result.Success(new ProductListPage(rows, page, pageSize, total));
    }
}
