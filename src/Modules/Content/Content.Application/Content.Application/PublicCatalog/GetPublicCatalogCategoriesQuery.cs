using Content.Application.Contracts;
using Content.Domain.ProductPages;

namespace Content.Application.PublicCatalog;

/// <summary>The category tree of the site with names in both languages (CNT-003 R-02, R-06).</summary>
public sealed record GetPublicCatalogCategoriesQuery : IQuery<Result<PublicCatalogCategoryTree>>;

public sealed class GetPublicCatalogCategoriesQueryHandler(IPublicCatalogSource source)
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
