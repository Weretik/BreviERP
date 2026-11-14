using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;

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
