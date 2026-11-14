using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;

/// <summary>
/// One page of product cards of the site, in the order of the Admin list (sewing first, by code).
/// <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>; <paramref name="Language"/> is <c>uk</c> or <c>ru</c>.
/// <paramref name="Category"/> is the address of a shown site category: its products and those of its subcategories
/// come back in the order of the category (CNT-003 R-03); an unknown or hidden category is «not found».
/// </summary>
public sealed record GetPublicCatalogProductsQuery(
    int Page, int PageSize, string Language, string? Type = null, int? SectionId = null, string? Category = null)
    : IQuery<Result<PublicCatalogCardPage>>
{
    public const int MaxPageSize = 100;
}

public sealed class GetPublicCatalogProductsValidator : AbstractValidator<GetPublicCatalogProductsQuery>
{
    public GetPublicCatalogProductsValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetPublicCatalogProductsQuery.MaxPageSize);
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
        RuleFor(x => x.Type).Must(type => type is null || GetPublicCatalogTreeHandler.Types.Contains(type))
            .WithMessage("Тип: Sewing або Ppe.");
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId is not null);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(ProductPageRules.SlugMaxLength)
            .When(x => x.Category is not null);
    }
}

public sealed class GetPublicCatalogProductsHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogProductsQuery, Result<PublicCatalogCardPage>>
{
    public async ValueTask<Result<PublicCatalogCardPage>> Handle(GetPublicCatalogProductsQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var listed = snapshot.Products;
        if (query.Category is not null)
        {
            var category = snapshot.Categories.FirstOrDefault(x => x.Category.Slug == query.Category);
            if (category is null) return Result.NotFound("Розділ не знайдено.");
            listed = category.Products;
        }

        var found = listed
            .Where(x => query.Type is null || x.Product.Type == query.Type)
            .Where(x => query.SectionId is null || x.Product.SectionId == query.SectionId)
            .ToArray();
        var items = found.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => PublicCatalogMapping.Card(x, query.Language)).ToArray();
        return new PublicCatalogCardPage(items, query.Page, query.PageSize, found.Length);
    }
}
