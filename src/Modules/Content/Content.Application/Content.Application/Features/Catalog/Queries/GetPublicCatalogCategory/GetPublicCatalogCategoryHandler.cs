using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;

public sealed class GetPublicCatalogCategoryHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogCategoryQuery, Result<PublicCatalogCategoryPage>>
{
    public async ValueTask<Result<PublicCatalogCategoryPage>> Handle(GetPublicCatalogCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var found = snapshot.Categories.FirstOrDefault(x => x.Category.Slug == query.Slug);
        if (found is null) return Result.NotFound("Розділ не знайдено.");

        var category = found.Category;
        var text = PublicCatalogMapping.TextFor(category, query.Language);
        var parent = snapshot.Categories.FirstOrDefault(x => x.Category.Id == category.ParentId);
        return new PublicCatalogCategoryPage(category.Slug, PublicCatalogMapping.Name(category, query.Language),
            text?.Language == query.Language ? query.Language : ProductPageRules.Ukrainian,
            parent is null ? null : PublicCatalogMapping.Link(parent.Category, query.Language),
            snapshot.Categories.Where(x => x.Category.ParentId == category.Id)
                .Select(x => new PublicCatalogChildCategory(x.Category.Slug,
                    PublicCatalogMapping.Name(x.Category, query.Language), x.Products.Count))
                .ToArray(),
            text?.SeoTitle, text?.SeoDescription, text?.Text ?? [], found.Products.Count, category.UpdatedAtUtc);
    }
}
