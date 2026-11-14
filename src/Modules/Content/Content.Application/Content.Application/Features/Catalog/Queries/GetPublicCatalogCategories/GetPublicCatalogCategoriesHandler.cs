using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;

public sealed class GetPublicCatalogCategoriesHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogCategoriesQuery, Result<PublicCatalogCategoryTree>>
{
    public async ValueTask<Result<PublicCatalogCategoryTree>> Handle(GetPublicCatalogCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        return new PublicCatalogCategoryTree(snapshot.Categories.Where(x => x.Category.ParentId is null)
            .Select(top => Row(top, snapshot.Categories.Where(x => x.Category.ParentId == top.Category.Id)
                .Select(child => Row(child, [])).ToArray()))
            .ToArray());
    }

    private static PublicCatalogCategory Row(PublicCatalogSnapshotCategory category,
        IReadOnlyList<PublicCatalogCategory> children) =>
        new(category.Category.Slug,
            PublicCatalogMapping.Name(category.Category, ProductPageRules.Ukrainian),
            PublicCatalogMapping.Name(category.Category, ProductPageRules.Russian),
            category.Products.Count, children);
}
