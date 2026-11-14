using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;

/// <summary>A product page of the site by its address. Unknown, hidden and inactive products are all «not found».</summary>
public sealed record GetPublicCatalogProductQuery(string Slug, string Language) : IQuery<Result<PublicCatalogProduct>>;

public sealed class GetPublicCatalogProductValidator : AbstractValidator<GetPublicCatalogProductQuery>
{
    public GetPublicCatalogProductValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(ProductPageRules.SlugMaxLength);
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
    }
}

public sealed class GetPublicCatalogProductHandler(IPublicCatalogSource source, IContentCatalogReader catalog)
    : IQueryHandler<GetPublicCatalogProductQuery, Result<PublicCatalogProduct>>
{
    public async ValueTask<Result<PublicCatalogProduct>> Handle(GetPublicCatalogProductQuery query,
        CancellationToken cancellationToken)
    {
        var snapshot = await source.GetAsync(cancellationToken);
        var entry = snapshot.Products.FirstOrDefault(x => x.Page.Slug == query.Slug);
        if (entry is null) return Result.NotFound("Товар не знайдено.");
        var section = snapshot.Sections.FirstOrDefault(x => x.Id == entry.Product.SectionId);
        var photos = await catalog.GetPhotosAsync(entry.Product.Id, cancellationToken);
        return PublicCatalogMapping.Product(entry, query.Language, section, photos,
            PublicCatalogMapping.CategoriesOf(snapshot, entry.Product.Id, query.Language));
    }
}
