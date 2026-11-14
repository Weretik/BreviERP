using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

public sealed class GetPublicArticlesValidator : AbstractValidator<GetPublicArticlesQuery>
{
    public GetPublicArticlesValidator() =>
        RuleFor(x => x.Language).Must(ProductPageRules.IsLanguage).WithMessage("Мова: uk або ru.");
}
