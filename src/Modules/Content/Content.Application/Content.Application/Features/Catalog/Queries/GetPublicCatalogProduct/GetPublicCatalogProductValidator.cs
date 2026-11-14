using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Catalog.Shared;

namespace Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;

public sealed class GetPublicCatalogProductValidator : AbstractValidator<GetPublicCatalogProductQuery>
{
    public GetPublicCatalogProductValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(ProductPageRules.SlugMaxLength);
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
    }
}
