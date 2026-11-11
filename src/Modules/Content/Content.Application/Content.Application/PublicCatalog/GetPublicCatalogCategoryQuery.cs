using Content.Application.Contracts;
using Content.Domain.ProductPages;

namespace Content.Application.PublicCatalog;

/// <summary>A category page of the site by its address: SEO, text, parent and subcategories. Hidden is «not found».</summary>
public sealed record GetPublicCatalogCategoryQuery(string Slug, string Language)
    : IQuery<Result<PublicCatalogCategoryPage>>;

public sealed class GetPublicCatalogCategoryQueryValidator : AbstractValidator<GetPublicCatalogCategoryQuery>
{
    public GetPublicCatalogCategoryQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(ProductPageRules.SlugMaxLength);
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
    }
}

public sealed class GetPublicCatalogCategoryQueryHandler(IPublicCatalogSource source)
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
