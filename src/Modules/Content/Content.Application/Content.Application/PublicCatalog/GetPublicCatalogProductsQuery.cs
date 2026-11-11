using Content.Application.Contracts;
using Content.Domain.ProductPages;

namespace Content.Application.PublicCatalog;

/// <summary>
/// One page of product cards of the site, in the order of the Admin list (sewing first, by code).
/// <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>; <paramref name="Language"/> is <c>uk</c> or <c>ru</c>.
/// </summary>
public sealed record GetPublicCatalogProductsQuery(
    int Page, int PageSize, string Language, string? Type = null, int? SectionId = null)
    : IQuery<Result<PublicCatalogCardPage>>
{
    public const int MaxPageSize = 100;
}

public sealed class GetPublicCatalogProductsQueryValidator : AbstractValidator<GetPublicCatalogProductsQuery>
{
    public GetPublicCatalogProductsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetPublicCatalogProductsQuery.MaxPageSize);
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
        RuleFor(x => x.Type).Must(type => type is null || GetPublicCatalogTreeQueryHandler.Types.Contains(type))
            .WithMessage("Тип: Sewing або Ppe.");
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId is not null);
    }
}

public sealed class GetPublicCatalogProductsQueryHandler(IPublicCatalogSource source)
    : IQueryHandler<GetPublicCatalogProductsQuery, Result<PublicCatalogCardPage>>
{
    public async ValueTask<Result<PublicCatalogCardPage>> Handle(GetPublicCatalogProductsQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var found = snapshot.Products
            .Where(x => query.Type is null || x.Product.Type == query.Type)
            .Where(x => query.SectionId is null || x.Product.SectionId == query.SectionId)
            .ToArray();
        var items = found.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => PublicCatalogMapping.Card(x, query.Language)).ToArray();
        return new PublicCatalogCardPage(items, query.Page, query.PageSize, found.Length);
    }
}
