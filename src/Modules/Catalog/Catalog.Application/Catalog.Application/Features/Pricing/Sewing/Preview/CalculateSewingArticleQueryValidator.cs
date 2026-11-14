using Catalog.Application.Features.Pricing.Validation;

namespace Catalog.Application.Features.Pricing.Sewing.Preview;

public sealed class CalculateSewingArticleQueryValidator : AbstractValidator<CalculateSewingArticleQuery>
{
    public CalculateSewingArticleQueryValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.ArticleId).ValidArticleId();
    }
}
